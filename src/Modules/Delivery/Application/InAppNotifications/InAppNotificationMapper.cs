using Delivery.Domain.InAppNotifications;

namespace Delivery.Application.InAppNotifications;

/// <summary>
/// Bộ ánh xạ dữ liệu Thông báo trong ứng dụng sang DTO phản hồi.
/// </summary>
internal static class InAppNotificationMapper
{
    /// <summary>
    /// Ánh xạ thực thể Thông báo trong ứng dụng sang DTO phản hồi.
    /// </summary>
    /// <param name="entity">Thực thể Thông báo trong ứng dụng.</param>
    /// <returns>DTO phản hồi.</returns>
    public static InAppNotificationResponseDto ToResponse(InAppNotification entity)
    {
        return new InAppNotificationResponseDto(
            entity.Id,
            entity.NotificationId,
            entity.RecipientUserId,
            entity.Title,
            entity.Content,
            entity.ReadStatus.ToString(),
            entity.CreatedAtUtc,
            entity.ReadAtUtc);
    }
}
