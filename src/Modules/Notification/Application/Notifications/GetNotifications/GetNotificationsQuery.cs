using NotificationChannel = global::Notification.Domain.NotificationChannel;
using NotificationStatus = global::Notification.Domain.NotificationStatus;

namespace Notification.Application.Notifications.GetNotifications;

/// <summary>
/// Truy vấn lấy danh sách Thông báo đã chuẩn hóa.
/// </summary>
/// <param name="Status">Trạng thái cần lọc.</param>
/// <param name="Channel">Kênh gửi cần lọc.</param>
/// <param name="CreatedFromUtc">Thời điểm tạo từ (UTC).</param>
/// <param name="CreatedToUtc">Thời điểm tạo đến (UTC).</param>
/// <param name="Page">Số trang.</param>
/// <param name="PageSize">Kích thước trang.</param>
/// <param name="ScopeRestriction">Giới hạn phạm vi truy cập.</param>
public sealed record GetNotificationsQuery(
    NotificationStatus? Status,
    NotificationChannel? Channel,
    DateTime? CreatedFromUtc,
    DateTime? CreatedToUtc,
    int Page,
    int PageSize,
    string? ScopeRestriction);