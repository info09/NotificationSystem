using Administration.Application.DeadLetterQueue;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Administration.Infrastructure.DeadLetterQueue;

/// <summary>
/// Cổng truy cập Hàng đợi thư chết bằng RabbitMQ Management HTTP API và AMQP.
/// </summary>
public sealed class RabbitMqDeadLetterQueueGateway : IDeadLetterQueueGateway
{
    private static readonly HashSet<string> AllowedQueues = new(StringComparer.OrdinalIgnoreCase)
    {
        "notifications.email.dlq",
        "notifications.inapp.dlq"
    };

    private static readonly JsonSerializerOptions ManagementJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AdministrationRabbitMqOptions _options;
    private readonly ILogger<RabbitMqDeadLetterQueueGateway> _logger;

    /// <summary>
    /// Khởi tạo cổng truy cập Hàng đợi thư chết.
    /// </summary>
    /// <param name="httpClientFactory">Factory tạo HTTP client.</param>
    /// <param name="options">Cấu hình RabbitMQ.</param>
    /// <param name="logger">Logger ghi nhật ký.</param>
    public RabbitMqDeadLetterQueueGateway(
        IHttpClientFactory httpClientFactory,
        IOptions<AdministrationRabbitMqOptions> options,
        ILogger<RabbitMqDeadLetterQueueGateway> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Lấy danh sách Thông điệp trong Hàng đợi thư chết.
    /// </summary>
    /// <param name="queueName">Tên Hàng đợi thư chết.</param>
    /// <param name="limit">Số lượng tối đa.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Danh sách Thông điệp.</returns>
    public async Task<IReadOnlyList<DeadLetterQueueMessageDto>> GetMessagesAsync(string queueName, int limit, CancellationToken cancellationToken)
    {
        EnsureQueueAllowed(queueName);

        var client = _httpClientFactory.CreateClient(nameof(RabbitMqDeadLetterQueueGateway));
        var vhost = Uri.EscapeDataString(_options.VirtualHost);
        var queue = Uri.EscapeDataString(queueName);
        var url = $"http://{_options.Host}:{_options.ManagementPort}/api/queues/{vhost}/{queue}/get";

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_options.UserName}:{_options.Password}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", auth);
        request.Content = new StringContent(
            JsonSerializer.Serialize(new
            {
                count = limit,
                ackmode = "ack_requeue_true",
                encoding = "auto",
                truncate = 50000
            }),
            Encoding.UTF8,
            "application/json");

        using var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var messages = await JsonSerializer.DeserializeAsync<List<ManagementQueueMessage>>(stream, ManagementJsonOptions, cancellationToken)
            ?? [];

        return messages.Select(m => MapMessage(queueName, m)).ToList();
    }

    /// <summary>
    /// Gửi lại Thông điệp từ Hàng đợi thư chết.
    /// </summary>
    /// <param name="queueName">Tên Hàng đợi thư chết.</param>
    /// <param name="messageId">Mã định danh Thông điệp.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả gửi lại, hoặc null nếu không tìm thấy.</returns>
    public async Task<DeadLetterResendGatewayResult?> ResendAsync(string queueName, string messageId, CancellationToken cancellationToken)
    {
        EnsureQueueAllowed(queueName);

        var factory = new ConnectionFactory
        {
            HostName = _options.Host,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password,
            VirtualHost = _options.VirtualHost
        };

        await using var connection = await factory.CreateConnectionAsync(cancellationToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        var scanned = 0;
        var maxScan = 200;
        var requeue = new List<(ulong DeliveryTag, BasicGetResult Result)>();

        try
        {
            while (scanned < maxScan)
            {
                var result = await channel.BasicGetAsync(queueName, autoAck: false, cancellationToken);
                if (result is null)
                {
                    break;
                }

                scanned++;
                var currentMessageId = result.BasicProperties?.MessageId;
                if (string.Equals(currentMessageId, messageId, StringComparison.OrdinalIgnoreCase))
                {
                    var resent = await RepublishAsync(channel, queueName, result, cancellationToken);
                    await channel.BasicAckAsync(result.DeliveryTag, multiple: false, cancellationToken);
                    return resent;
                }

                requeue.Add((result.DeliveryTag, result));
            }
        }
        finally
        {
            foreach (var item in requeue)
            {
                try
                {
                    await channel.BasicNackAsync(item.DeliveryTag, multiple: false, requeue: true, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Không thể Nack requeue Thông điệp DLQ. Queue={QueueName}, DeliveryTag={DeliveryTag}", queueName, item.DeliveryTag);
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Phát hành lại Thông điệp DLQ với MessageId mới.
    /// </summary>
    /// <param name="channel">Kênh RabbitMQ.</param>
    /// <param name="queueName">Tên Hàng đợi thư chết.</param>
    /// <param name="result">Thông điệp gốc từ DLQ.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả gửi lại.</returns>
    private static async Task<DeadLetterResendGatewayResult> RepublishAsync(
        IChannel channel,
        string queueName,
        BasicGetResult result,
        CancellationToken cancellationToken)
    {
        var payload = Encoding.UTF8.GetString(result.Body.ToArray());
        var notificationId = TryGetNotificationId(payload);
        var routingKey = ResolveRoutingKey(result, queueName);
        var exchange = ResolveExchange(result);

        var resentMessageId = Guid.NewGuid();
        var properties = new BasicProperties
        {
            Persistent = true,
            MessageId = resentMessageId.ToString(),
            ContentType = result.BasicProperties?.ContentType ?? "application/json",
            Type = result.BasicProperties?.Type,
            Headers = result.BasicProperties?.Headers is null
                ? new Dictionary<string, object?>()
                : new Dictionary<string, object?>(result.BasicProperties.Headers)
        };
        properties.Headers["resendOfMessageId"] = result.BasicProperties?.MessageId ?? string.Empty;
        properties.Headers["resendFromQueue"] = queueName;

        await channel.BasicPublishAsync(
            exchange: exchange,
            routingKey: routingKey,
            mandatory: false,
            basicProperties: properties,
            body: result.Body,
            cancellationToken: cancellationToken);

        return new DeadLetterResendGatewayResult(
            result.BasicProperties?.MessageId ?? string.Empty,
            resentMessageId,
            notificationId,
            queueName);
    }

    /// <summary>
    /// Ánh xạ thông điệp từ RabbitMQ Management API sang DTO.
    /// </summary>
    /// <param name="queueName">Tên Hàng đợi thư chết.</param>
    /// <param name="message">Thông điệp từ Management API.</param>
    /// <returns>DTO Thông điệp DLQ.</returns>
    private static DeadLetterQueueMessageDto MapMessage(string queueName, ManagementQueueMessage message)
    {
        var payload = message.Payload;
        var notificationId = TryGetNotificationId(payload)?.ToString();
        var eventType = TryGetEventType(payload);
        var (reason, deadLetteredAtUtc, deathCount, routingKey) = ExtractDeathInfo(message);

        return new DeadLetterQueueMessageDto(
            QueueName: queueName,
            MessageId: message.Properties?.MessageId ?? string.Empty,
            NotificationId: notificationId,
            Channel: TryGetChannel(payload),
            RoutingKey: routingKey ?? message.RoutingKey,
            EventType: eventType,
            Reason: reason,
            DeadLetteredAtUtc: deadLetteredAtUtc,
            DeathCount: deathCount,
            Payload: payload);
    }

    /// <summary>
    /// Trích xuất thông tin dead-letter từ headers.
    /// </summary>
    /// <param name="message">Thông điệp từ Management API.</param>
    /// <returns>Thông tin dead-letter.</returns>
    private static (string? Reason, DateTime? DeadLetteredAtUtc, long? DeathCount, string? RoutingKey) ExtractDeathInfo(ManagementQueueMessage message)
    {
        if (message.Properties?.Headers is null ||
            !message.Properties.Headers.TryGetValue("x-death", out var xDeathObj) ||
            xDeathObj is not JsonElement { ValueKind: JsonValueKind.Array } xDeathArray ||
            xDeathArray.GetArrayLength() == 0)
        {
            return (null, null, null, null);
        }

        var first = xDeathArray[0];
        string? reason = first.TryGetProperty("reason", out var reasonProp) ? reasonProp.GetString() : null;
        long? count = first.TryGetProperty("count", out var countProp) && countProp.TryGetInt64(out var c) ? c : null;

        DateTime? deadLetteredAtUtc = null;
        if (first.TryGetProperty("time", out var timeProp))
        {
            if (timeProp.ValueKind == JsonValueKind.Number && timeProp.TryGetInt64(out var unix))
            {
                deadLetteredAtUtc = DateTimeOffset.FromUnixTimeSeconds(unix).UtcDateTime;
            }
            else if (timeProp.ValueKind == JsonValueKind.String && DateTime.TryParse(timeProp.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var parsed))
            {
                deadLetteredAtUtc = parsed;
            }
        }

        string? routingKey = null;
        if (first.TryGetProperty("routing-keys", out var rkProp) && rkProp.ValueKind == JsonValueKind.Array && rkProp.GetArrayLength() > 0)
        {
            routingKey = rkProp[0].GetString();
        }

        return (reason, deadLetteredAtUtc, count, routingKey);
    }

    /// <summary>
    /// Lấy NotificationId từ payload JSON.
    /// </summary>
    /// <param name="payload">Payload JSON.</param>
    /// <returns>NotificationId nếu có.</returns>
    private static Guid? TryGetNotificationId(string? payload)
    {
        if (string.IsNullOrWhiteSpace(payload))
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(payload);
            if (doc.RootElement.TryGetProperty("notificationId", out var prop) &&
                prop.ValueKind == JsonValueKind.String &&
                Guid.TryParse(prop.GetString(), out var id))
            {
                return id;
            }
        }
        catch (JsonException)
        {
            return null;
        }

        return null;
    }

    /// <summary>
    /// Lấy loại sự kiện từ payload JSON.
    /// </summary>
    /// <param name="payload">Payload JSON.</param>
    /// <returns>Loại sự kiện nếu có.</returns>
    private static string? TryGetEventType(string? payload)
    {
        if (string.IsNullOrWhiteSpace(payload))
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(payload);
            if (doc.RootElement.TryGetProperty("eventType", out var prop) && prop.ValueKind == JsonValueKind.String)
            {
                return prop.GetString();
            }
        }
        catch (JsonException)
        {
            return null;
        }

        return null;
    }

    /// <summary>
    /// Lấy kênh gửi từ payload JSON.
    /// </summary>
    /// <param name="payload">Payload JSON.</param>
    /// <returns>Kênh gửi nếu có.</returns>
    private static string? TryGetChannel(string? payload)
    {
        if (string.IsNullOrWhiteSpace(payload))
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(payload);
            if (doc.RootElement.TryGetProperty("channel", out var prop) && prop.ValueKind == JsonValueKind.String)
            {
                return prop.GetString();
            }
        }
        catch (JsonException)
        {
            return null;
        }

        return null;
    }

    /// <summary>
    /// Xác định routing key để phát hành lại Thông điệp.
    /// </summary>
    /// <param name="result">Thông điệp từ DLQ.</param>
    /// <param name="queueName">Tên Hàng đợi thư chết.</param>
    /// <returns>Routing key.</returns>
    private static string ResolveRoutingKey(BasicGetResult result, string queueName)
    {
        if (result.BasicProperties?.Headers is not null &&
            result.BasicProperties.Headers.TryGetValue("x-death", out var xDeath) &&
            xDeath is IList<object> list &&
            list.Count > 0 &&
            list[0] is IDictionary<string, object> first &&
            first.TryGetValue("routing-keys", out var rk) &&
            rk is IList<object> rks &&
            rks.Count > 0)
        {
            return Encoding.UTF8.GetString((byte[])rks[0]);
        }

        return queueName.Contains("email", StringComparison.OrdinalIgnoreCase)
            ? "notification.email.created"
            : "notification.inapp.created";
    }

    /// <summary>
    /// Xác định exchange để phát hành lại Thông điệp.
    /// </summary>
    /// <param name="result">Thông điệp từ DLQ.</param>
    /// <returns>Tên exchange.</returns>
    private static string ResolveExchange(BasicGetResult result)
    {
        if (result.BasicProperties?.Headers is not null &&
            result.BasicProperties.Headers.TryGetValue("x-death", out var xDeath) &&
            xDeath is IList<object> list &&
            list.Count > 0 &&
            list[0] is IDictionary<string, object> first &&
            first.TryGetValue("exchange", out var ex) &&
            ex is byte[] bytes)
        {
            var exchange = Encoding.UTF8.GetString(bytes);
            if (!string.IsNullOrWhiteSpace(exchange))
            {
                return exchange;
            }
        }

        return "notifications.topic";
    }

    /// <summary>
    /// Kiểm tra tên Hàng đợi thư chết có được hỗ trợ hay không.
    /// </summary>
    /// <param name="queueName">Tên Hàng đợi thư chết.</param>
    private static void EnsureQueueAllowed(string queueName)
    {
        if (!AllowedQueues.Contains(queueName))
        {
            throw new ArgumentOutOfRangeException(nameof(queueName), "Queue name is not supported.");
        }
    }

    /// <summary>
    /// Mô hình Thông điệp từ RabbitMQ Management API.
    /// </summary>
    private sealed class ManagementQueueMessage
    {
        /// <summary>
        /// Payload Thông điệp.
        /// </summary>
        [JsonPropertyName("payload")]
        public string? Payload { get; set; }

        /// <summary>
        /// Routing key.
        /// </summary>
        [JsonPropertyName("routing_key")]
        public string? RoutingKey { get; set; }

        /// <summary>
        /// Thuộc tính Thông điệp.
        /// </summary>
        [JsonPropertyName("properties")]
        public ManagementProperties? Properties { get; set; }
    }

    /// <summary>
    /// Mô hình thuộc tính Thông điệp từ RabbitMQ Management API.
    /// </summary>
    private sealed class ManagementProperties
    {
        /// <summary>
        /// Mã định danh Thông điệp.
        /// </summary>
        [JsonPropertyName("message_id")]
        public string? MessageId { get; set; }

        /// <summary>
        /// Headers của Thông điệp.
        /// </summary>
        [JsonPropertyName("headers")]
        public Dictionary<string, JsonElement>? Headers { get; set; }
    }
}
