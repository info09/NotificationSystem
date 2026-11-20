namespace Notification.Presentation;

public sealed record NotificationResponseItem(
    Guid Id,
    string Recipient,
    string Channel,
    string Status,
    string? Content,
    Guid? NotificationTemplateId,
    DateTime CreatedAt);