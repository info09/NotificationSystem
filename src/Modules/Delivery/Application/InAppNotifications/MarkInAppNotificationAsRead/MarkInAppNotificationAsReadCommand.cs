namespace Delivery.Application.InAppNotifications.MarkInAppNotificationAsRead;

/// <summary>
/// Lệnh đánh dấu Thông báo trong ứng dụng là đã đọc.
/// </summary>
/// <param name="Id">Mã định danh Thông báo trong ứng dụng.</param>
/// <param name="RecipientUserId">Mã định danh Người dùng cuối.</param>
public sealed record MarkInAppNotificationAsReadCommand(Guid Id, string? RecipientUserId);
