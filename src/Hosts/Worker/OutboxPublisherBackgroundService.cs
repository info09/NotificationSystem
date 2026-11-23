using IntegrationMessaging.Domain;
using IntegrationMessaging.Infrastructure.Messaging;
using IntegrationMessaging.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Worker;

/// <summary>
/// BackgroundService publish các Bản ghi Outbox chưa xử lý sang RabbitMQ.
/// </summary>
public sealed class OutboxPublisherBackgroundService : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(5);
    private const int BatchSize = 20;

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

        var pendingMessages = await dbContext.OutboxMessages
            .Where(x => x.Status == OutboxMessageStatus.Pending)
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
                _logger.LogWarning("Không xác định được routing key cho OutboxMessage {OutboxMessageId}.", message.Id);
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
                _logger.LogError(ex, "Publish OutboxMessage {OutboxMessageId} thất bại.", message.Id);
            }
        }

        return publishedCount;
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
