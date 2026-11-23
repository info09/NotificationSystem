namespace Delivery.Application.InAppNotifications.CreateInAppNotification;

/// <summary>
/// Lệnh tạo Thông báo trong ứng dụng.
/// </summary>
/// <param name="NotificationId">Mã định danh Thông báo gốc.</param>
/// <param name="RecipientUserId">Mã định danh Người dùng cuối nhận Thông báo.</param>
/// <param name="Title">Tiêu đề Thông báo trong ứng dụng.</param>
/// <param name="Content">Nội dung Thông báo trong ứng dụng.</param>
public sealed record CreateInAppNotificationCommand(
    Guid NotificationId,
    string? RecipientUserId,
    string? Title,
    string? Content);
