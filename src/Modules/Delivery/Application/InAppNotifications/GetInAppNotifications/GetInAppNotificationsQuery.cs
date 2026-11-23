namespace Delivery.Application.InAppNotifications.GetInAppNotifications;

/// <summary>
/// Truy vấn lấy danh sách Thông báo trong ứng dụng.
/// </summary>
/// <param name="RecipientUserId">Mã định danh Người dùng cuối.</param>
/// <param name="UnreadOnly">Chỉ lấy Thông báo chưa đọc.</param>
/// <param name="Page">Số trang.</param>
/// <param name="PageSize">Kích thước trang.</param>
public sealed record GetInAppNotificationsQuery(
    string? RecipientUserId,
    bool UnreadOnly,
    int Page,
    int PageSize);
