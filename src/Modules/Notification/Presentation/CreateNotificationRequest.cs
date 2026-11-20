namespace Notification.Presentation;

public sealed record CreateNotificationRequest(
    string? IdempotencyKey,
    string? Recipient,
    string? Channel,
    string? Content,
    Guid? NotificationTemplateId);