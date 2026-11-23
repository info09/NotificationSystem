namespace Notification.Application.Notifications.GetNotifications;

/// <summary>
/// Yêu cầu lấy danh sách Thông báo có lọc và phân trang.
/// </summary>
/// <param name="Status">Trạng thái cần lọc.</param>
/// <param name="Channel">Kênh gửi cần lọc.</param>
/// <param name="CreatedFrom">Thời điểm tạo từ.</param>
/// <param name="CreatedTo">Thời điểm tạo đến.</param>
/// <param name="Page">Số trang.</param>
/// <param name="PageSize">Kích thước trang.</param>
/// <param name="Caller">Ngữ cảnh người gọi.</param>
public sealed record GetNotificationsAppRequest(
    string? Status,
    string? Channel,
    string? CreatedFrom,
    string? CreatedTo,
    int Page,
    int PageSize,
    NotificationCallerContext Caller);