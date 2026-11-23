namespace Delivery.Application.InAppNotifications.GetUnreadInAppNotificationCount;

/// <summary>
/// Truy vấn lấy số lượng Thông báo trong ứng dụng chưa đọc.
/// </summary>
/// <param name="RecipientUserId">Mã định danh Người dùng cuối.</param>
public sealed record GetUnreadInAppNotificationCountQuery(string? RecipientUserId);
