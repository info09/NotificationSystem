using DomainNotification = Notification.Domain.Notification;

namespace Notification.Application.Notifications;

public static class NotificationMapper
{
    internal static NotificationDto MapToDto(DomainNotification notification)
    {
        return new NotificationDto(
            notification.Id,
            notification.Recipient.Value,
            notification.Channel,
            notification.Status,
            notification.Content,
            notification.NotificationTemplateId);
    }
}
