using DomainNotification = global::Notification.Domain.Notification;

namespace Notification.Application.Notifications;

/// <summary>
/// Bộ ánh xạ dữ liệu Thông báo giữa domain và DTO/response.
/// </summary>
public static class NotificationMapper
{
    /// <summary>
    /// Ánh xạ thực thể Thông báo sang DTO.
    /// </summary>
    /// <param name="notification">Thực thể Thông báo.</param>
    /// <returns>DTO Thông báo.</returns>
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

    /// <summary>
    /// Ánh xạ thực thể Thông báo sang DTO phản hồi API.
    /// </summary>
    /// <param name="notification">Thực thể Thông báo.</param>
    /// <returns>DTO phản hồi Thông báo.</returns>
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
