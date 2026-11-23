namespace Delivery.Application.DeliveryLogs.GetNotificationDeliveryHistory;

/// <summary>
/// Truy vấn lấy lịch sử gửi theo mã định danh Thông báo.
/// </summary>
/// <param name="NotificationId">Mã định danh Thông báo.</param>
/// <param name="Channel">Kênh gửi cần lọc.</param>
/// <param name="Result">Kết quả cần lọc.</param>
/// <param name="Page">Số trang.</param>
/// <param name="PageSize">Kích thước trang.</param>
public sealed record GetNotificationDeliveryHistoryQuery(
    Guid NotificationId,
    string? Channel = null,
    string? Result = null,
    int Page = 1,
    int PageSize = 20);
