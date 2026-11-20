using NotificationChannel = Notification.Domain.NotificationChannel;

namespace Notification.Application.Notifications.CreateNotification;

public sealed record CreateNotificationCommand(
    string Recipient,
    NotificationChannel Channel,
    string? Content,
    Guid? NotificationTemplateId);
