namespace Delivery.Application.InAppNotifications.GetInAppNotificationById;

/// <summary>
/// Truy vấn lấy chi tiết Thông báo trong ứng dụng theo mã định danh.
/// </summary>
/// <param name="Id">Mã định danh Thông báo trong ứng dụng.</param>
/// <param name="RecipientUserId">Mã định danh Người dùng cuối.</param>
public sealed record GetInAppNotificationByIdQuery(Guid Id, string? RecipientUserId);
