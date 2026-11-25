using IntegrationMessaging.Domain;
using IntegrationMessaging.Infrastructure.Messaging;
using IntegrationMessaging.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Worker.Outbox;

/// <summary>
/// BackgroundService publish các Bản ghi Outbox chưa xử lý sang RabbitMQ.
/// </summary>
public sealed class OutboxPublisherBackgroundService : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(5);
    private const int BatchSize = 20;
    private const int MaxRetryCount = 10;
    private static readonly TimeSpan BaseRetryDelay = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan MaxRetryDelay = TimeSpan.FromMinutes(5);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IRabbitMqMessagePublisher _messagePublisher;
    private readonly ILogger<OutboxPublisherBackgroundService> _logger;

    /// <summary>
    /// Khởi tạo Outbox Publisher.
    /// </summary>
    /// <param name="scopeFactory">Factory tạo scope dịch vụ.</param>
    /// <param name="messagePublisher">Publisher RabbitMQ.</param>
    /// <param name="logger">Logger ghi nhật ký.</param>
    public OutboxPublisherBackgroundService(
        IServiceScopeFactory scopeFactory,
        IRabbitMqMessagePublisher messagePublisher,
        ILogger<OutboxPublisherBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _messagePublisher = messagePublisher;
        _logger = logger;
    }

    /// <summary>
    /// Thực thi vòng lặp publish OutboxMessage.
    /// </summary>
    /// <param name="stoppingToken">Token dừng dịch vụ.</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PublishPendingMessagesAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi publish OutboxMessage.");
            }

            try
            {
                await Task.Delay(PollInterval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    /// <summary>
    /// Đọc và publish các bản ghi Outbox đang chờ.
    /// </summary>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    internal async Task<int> PublishPendingMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IntegrationMessagingDbContext>();
        var nowUtc = DateTime.UtcNow;

        var pendingMessages = await dbContext.OutboxMessages
            .Where(x => x.Status == OutboxMessageStatus.Pending && x.NextAttemptAtUtc <= nowUtc)
            .OrderBy(x => x.OccurredAtUtc)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        var publishedCount = 0;

        foreach (var message in pendingMessages)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var routingKey = ResolveRoutingKey(message);
            if (routingKey is null)
            {
                var error = "Không xác định được routing key cho OutboxMessage.";
                _logger.LogWarning("{Error} OutboxMessageId={OutboxMessageId}", error, message.Id);
                message.MarkPublishFailed(error, DateTime.UtcNow, DateTime.UtcNow, maxRetryCount: 1);
                await dbContext.SaveChangesAsync(cancellationToken);
                continue;
            }

            try
            {
                await _messagePublisher.PublishAsync(
                    RabbitMqTopologyNames.NotificationsExchange,
                    routingKey,
                    message.Payload,
                    message.MessageId,
                    cancellationToken);

                message.MarkPublished(DateTime.UtcNow);
                await dbContext.SaveChangesAsync(cancellationToken);
                publishedCount++;
            }
            catch (Exception ex)
            {
                var attemptedAtUtc = DateTime.UtcNow;
                var nextAttemptAtUtc = CalculateNextAttemptAtUtc(message.RetryCount + 1, attemptedAtUtc);
                message.MarkPublishFailed(ex.Message, attemptedAtUtc, nextAttemptAtUtc, MaxRetryCount);
                await dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogError(
                    ex,
                    "Publish OutboxMessage {OutboxMessageId} thất bại. RetryCount={RetryCount}, NextAttemptAtUtc={NextAttemptAtUtc}, Status={Status}",
                    message.Id,
                    message.RetryCount,
                    message.NextAttemptAtUtc,
                    message.Status);
            }
        }

        return publishedCount;
    }

    /// <summary>
    /// Tính thời điểm thử publish tiếp theo theo exponential backoff.
    /// </summary>
    /// <param name="retryCount">Số lần thử thất bại sau khi cập nhật.</param>
    /// <param name="attemptedAtUtc">Thời điểm thử hiện tại (UTC).</param>
    /// <returns>Thời điểm thử lại tiếp theo (UTC).</returns>
    internal static DateTime CalculateNextAttemptAtUtc(int retryCount, DateTime attemptedAtUtc)
    {
        var exponent = Math.Clamp(retryCount - 1, 0, 10);
        var delaySeconds = BaseRetryDelay.TotalSeconds * Math.Pow(2, exponent);
        var delay = TimeSpan.FromSeconds(Math.Min(delaySeconds, MaxRetryDelay.TotalSeconds));
        return attemptedAtUtc.Add(delay);
    }

    /// <summary>
    /// Xác định routing key từ nội dung OutboxMessage.
    /// </summary>
    /// <param name="message">Bản ghi Outbox.</param>
    /// <returns>Routing key hoặc null nếu không xác định được.</returns>
    internal static string? ResolveRoutingKey(OutboxMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        try
        {
            using var document = JsonDocument.Parse(message.Payload);
            if (!document.RootElement.TryGetProperty("channel", out var channelElement))
            {
                return null;
            }

            var channel = channelElement.GetString();
            if (string.Equals(channel, "Email", StringComparison.OrdinalIgnoreCase))
            {
                return "notification.email.created";
            }

            if (string.Equals(channel, "InApp", StringComparison.OrdinalIgnoreCase))
            {
                return "notification.inapp.created";
            }

            return null;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
