using System.Text.Json;
using Notification.Application.Notifications;
using DomainNotification = global::Notification.Domain.Notification;

namespace Notification.Infrastructure.Persistence;

/// <summary>
/// Ghi bản ghi Outbox cho các sự kiện nghiệp vụ của Thông báo.
/// </summary>
public sealed class NotificationOutboxWriter : INotificationOutboxWriter
{
    private readonly NotificationDbContext _dbContext;

    /// <summary>
    /// Khởi tạo writer với DbContext của module Notification.
    /// </summary>
    /// <param name="dbContext">DbContext của module Notification.</param>
    public NotificationOutboxWriter(NotificationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Thêm bản ghi Outbox cho sự kiện tạo Thông báo.
    /// </summary>
    /// <param name="notification">Thông báo vừa được tạo.</param>
    public void AddNotificationCreated(DomainNotification notification)
    {
        ArgumentNullException.ThrowIfNull(notification);

        var payload = JsonSerializer.Serialize(new
        {
            notificationId = notification.Id,
            recipient = notification.Recipient.Value,
            channel = notification.Channel.ToString(),
            status = notification.Status.ToString(),
            content = notification.Content,
            notificationTemplateId = notification.NotificationTemplateId,
            idempotencyScope = notification.IdempotencyScope,
            idempotencyKey = notification.IdempotencyKey,
            createdAt = notification.CreatedAt
        });

        var outboxMessage = new OutboxMessageRecord
        {
            Id = Guid.NewGuid(),
            MessageId = Guid.NewGuid(),
            EventType = "NotificationCreated",
            Payload = payload,
            Status = "Pending",
            OccurredAtUtc = notification.CreatedAt,
            RetryCount = 0,
            NextAttemptAtUtc = notification.CreatedAt,
            LastError = null
        };

        _dbContext.Set<OutboxMessageRecord>().Add(outboxMessage);
    }
}
