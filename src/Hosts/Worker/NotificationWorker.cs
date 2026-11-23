using IntegrationMessaging.Domain;
using IntegrationMessaging.Infrastructure.Messaging;
using IntegrationMessaging.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Worker;

/// <summary>
/// Worker tiêu thụ Thông điệp từ Queue, kiểm tra idempotency và ack sau xử lý an toàn.
/// </summary>
public sealed class NotificationWorker : BackgroundService
{
    private const string ConsumerName = "NotificationWorker";

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMqOptions _options;
    private readonly ILogger<NotificationWorker> _logger;
    private readonly SemaphoreSlim _channelSemaphore = new(1, 1);

    internal Func<int, TimeSpan> RetryDelaySelector { get; set; } = attempt => NotificationWorkerRetryPolicy.GetDelay(attempt);

    /// <summary>
    /// Khởi tạo NotificationWorker.
    /// </summary>
    /// <param name="scopeFactory">Factory tạo scope dịch vụ.</param>
    /// <param name="options">Cấu hình RabbitMQ.</param>
    /// <param name="logger">Logger ghi nhật ký.</param>
    public NotificationWorker(
        IServiceScopeFactory scopeFactory,
        IOptions<RabbitMqOptions> options,
        ILogger<NotificationWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Thực thi vòng lặp consume RabbitMQ.
    /// </summary>
    /// <param name="stoppingToken">Token dừng dịch vụ.</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ConsumeAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi trong NotificationWorker. Sẽ thử kết nối lại.");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private async Task ConsumeAsync(CancellationToken stoppingToken)
    {
        var concurrency = Math.Max(1, _options.ConsumerConcurrency);
        var factory = new ConnectionFactory
        {
            HostName = _options.Host,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password,
            VirtualHost = _options.VirtualHost,
            ConsumerDispatchConcurrency = (ushort)concurrency
        };

        await using var connection = await factory.CreateConnectionAsync(stoppingToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await channel.BasicQosAsync(0, (ushort)concurrency, false, stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += (_, ea) => HandleMessageAsync(channel, ea, stoppingToken);

        await channel.BasicConsumeAsync(RabbitMqTopologyNames.EmailQueue, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);
        await channel.BasicConsumeAsync(RabbitMqTopologyNames.InAppQueue, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);

        _logger.LogInformation("NotificationWorker đã bắt đầu consume queue.");

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    internal async Task HandleMessageAsync(IChannel channel, BasicDeliverEventArgs ea, CancellationToken cancellationToken)
    {
        var messageId = ResolveMessageId(ea);
        if (messageId == Guid.Empty)
        {
            _logger.LogWarning("Bỏ qua Thông điệp không có MessageId hợp lệ. DeliveryTag={DeliveryTag}", ea.DeliveryTag);
            await AckAsync(channel, ea.DeliveryTag, cancellationToken);
            return;
        }

        var attempt = 0;
        while (true)
        {
            try
            {
                await TryProcessAsync(messageId, ea, cancellationToken);
                await AckAsync(channel, ea.DeliveryTag, cancellationToken);
                return;
            }
            catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
            {
                var retryable = NotificationWorkerRetryPolicy.IsRetryable(ex);
                if (!retryable || attempt >= NotificationWorkerRetryPolicy.MaxRetryCount)
                {
                    _logger.LogError(ex, "Xử lý Thông điệp thất bại, chuyển DLQ. MessageId={MessageId}, DeliveryTag={DeliveryTag}, Attempt={Attempt}, Retryable={Retryable}", messageId, ea.DeliveryTag, attempt, retryable);
                    await NackAsync(channel, ea.DeliveryTag, cancellationToken);
                    return;
                }

                attempt++;
                var delay = RetryDelaySelector(attempt);
                _logger.LogWarning(ex, "Xử lý Thông điệp thất bại, sẽ retry. MessageId={MessageId}, DeliveryTag={DeliveryTag}, Attempt={Attempt}, DelayMs={DelayMs}", messageId, ea.DeliveryTag, attempt, delay.TotalMilliseconds);
                await Task.Delay(delay, cancellationToken);
            }
        }
    }

    private async Task<bool> TryProcessAsync(Guid messageId, BasicDeliverEventArgs ea, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IntegrationMessagingDbContext>();

        var alreadyProcessed = await dbContext.InboxMessages
            .AsNoTracking()
            .AnyAsync(x => x.MessageId == messageId && x.ConsumerName == ConsumerName, cancellationToken);

        if (alreadyProcessed)
        {
            _logger.LogInformation("Bỏ qua Thông điệp trùng. MessageId={MessageId}, ConsumerName={ConsumerName}", messageId, ConsumerName);
            await TryFinalizeProcessedNotificationAsync(scope, ea, cancellationToken);
            return false;
        }

        var notificationId = ResolveNotificationId(ea);
        var notificationRepository = scope.ServiceProvider.GetRequiredService<Notification.Application.Notifications.INotificationRepository>();
        var notification = notificationId == Guid.Empty
            ? null
            : await notificationRepository.GetByIdAsync(notificationId, cancellationToken);

        if (notification is not null && notification.Status == global::Notification.Domain.NotificationStatus.Pending)
        {
            notification.MarkProcessing();
            await notificationRepository.SaveChangesAsync(cancellationToken);
        }

        var eventType = ResolveEventType(ea);
        var inboxMessage = InboxMessage.Create(Guid.NewGuid(), messageId, ConsumerName, eventType, DateTime.UtcNow);
        dbContext.InboxMessages.Add(inboxMessage);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            var isDuplicate = await dbContext.InboxMessages
                .AsNoTracking()
                .AnyAsync(x => x.MessageId == messageId && x.ConsumerName == ConsumerName, cancellationToken);

            if (isDuplicate)
            {
                _logger.LogInformation(ex, "Phát hiện trùng khi lưu InboxMessage. MessageId={MessageId}", messageId);
                return false;
            }

            throw;
        }

        if (notification is not null && notification.Status == global::Notification.Domain.NotificationStatus.Processing)
        {
            notification.MarkSent();
            await notificationRepository.SaveChangesAsync(cancellationToken);
        }

        _logger.LogInformation("Đã xử lý an toàn Thông điệp. MessageId={MessageId}, ConsumerName={ConsumerName}", messageId, ConsumerName);
        return true;
    }

    private async Task AckAsync(IChannel channel, ulong deliveryTag, CancellationToken cancellationToken)
    {
        await _channelSemaphore.WaitAsync(cancellationToken);
        try
        {
            await channel.BasicAckAsync(deliveryTag, multiple: false, cancellationToken);
        }
        finally
        {
            _channelSemaphore.Release();
        }
    }

    private async Task NackAsync(IChannel channel, ulong deliveryTag, CancellationToken cancellationToken)
    {
        await _channelSemaphore.WaitAsync(cancellationToken);
        try
        {
            await channel.BasicNackAsync(deliveryTag, multiple: false, requeue: false, cancellationToken);
        }
        finally
        {
            _channelSemaphore.Release();
        }
    }

    internal static Guid ResolveMessageId(BasicDeliverEventArgs ea)
    {
        if (Guid.TryParse(ea.BasicProperties.MessageId, out var messageId))
        {
            return messageId;
        }

        return Guid.Empty;
    }

    internal static Guid ResolveNotificationId(BasicDeliverEventArgs ea)
    {
        var body = Encoding.UTF8.GetString(ea.Body.ToArray());
        if (string.IsNullOrWhiteSpace(body))
        {
            return Guid.Empty;
        }

        try
        {
            using var document = JsonDocument.Parse(body);
            if (document.RootElement.TryGetProperty("notificationId", out var notificationIdElement)
                && Guid.TryParse(notificationIdElement.GetString(), out var notificationId))
            {
                return notificationId;
            }
        }
        catch (JsonException)
        {
            return Guid.Empty;
        }

        return Guid.Empty;
    }

    internal static string ResolveEventType(BasicDeliverEventArgs ea)
    {
        var body = Encoding.UTF8.GetString(ea.Body.ToArray());
        if (string.IsNullOrWhiteSpace(body))
        {
            return "Unknown";
        }

        return "NotificationCreated";
    }

    private async Task TryFinalizeProcessedNotificationAsync(IServiceScope scope, BasicDeliverEventArgs ea, CancellationToken cancellationToken)
    {
        var notificationId = ResolveNotificationId(ea);
        if (notificationId == Guid.Empty)
        {
            return;
        }

        var notificationRepository = scope.ServiceProvider.GetRequiredService<Notification.Application.Notifications.INotificationRepository>();
        var notification = await notificationRepository.GetByIdAsync(notificationId, cancellationToken);
        if (notification is not null && notification.Status == global::Notification.Domain.NotificationStatus.Processing)
        {
            notification.MarkSent();
            await notificationRepository.SaveChangesAsync(cancellationToken);
        }
    }
}
