namespace Notification.Application.Notifications.CreateNotification;

public sealed record CreateNotificationAppRequest(
    string? IdempotencyKey,
    string? Recipient,
    string? Channel,
    string? Content,
    Guid? NotificationTemplateId,
    NotificationCallerContext Caller);