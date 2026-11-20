using DomainNotification = global::Notification.Domain.Notification;

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
            notification.NotificationTemplateId,
            notification.CreatedAt);
    }

    internal static NotificationItemResponseDto MapToItemResponse(DomainNotification notification)
    {
        return new NotificationItemResponseDto(
            notification.Id,
            notification.Recipient.Value,
            notification.Channel.ToString(),
            notification.Status.ToString(),
            notification.Content,
            notification.NotificationTemplateId,
            notification.CreatedAt);
    }
}
