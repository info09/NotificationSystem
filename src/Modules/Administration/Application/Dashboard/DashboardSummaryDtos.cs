namespace Administration.Application.Dashboard;

/// <summary>
/// Tổng hợp số lượng Thông báo theo Trạng thái thông báo.
/// </summary>
/// <param name="Pending">Số Thông báo đang Chờ xử lý.</param>
/// <param name="Processing">Số Thông báo đang Đang xử lý.</param>
/// <param name="Sent">Số Thông báo Đã gửi.</param>
/// <param name="Failed">Số Thông báo Thất bại.</param>
/// <param name="Error">Lỗi khi tải dữ liệu, nếu có.</param>
public sealed record NotificationStatusSummaryDto(int Pending, int Processing, int Sent, int Failed, string? Error = null);

/// <summary>
/// Tổng hợp số lượng Thông điệp trong Hàng đợi thư chết.
/// </summary>
/// <param name="QueueName">Tên Hàng đợi thư chết.</param>
/// <param name="Count">Số lượng Thông điệp.</param>
/// <param name="Error">Lỗi khi tải dữ liệu, nếu có.</param>
public sealed record DeadLetterQueueSummaryDto(string QueueName, int Count, string? Error = null);

/// <summary>
/// Tổng hợp trạng thái Kiểm tra sức khỏe.
/// </summary>
/// <param name="Status">Trạng thái tổng hợp.</param>
/// <param name="Details">Chi tiết trạng thái.</param>
public sealed record HealthSummaryDto(string Status, string? Details);

/// <summary>
/// Phản hồi tổng hợp dữ liệu Tổng quan.
/// </summary>
/// <param name="Notifications">Tổng hợp Trạng thái thông báo.</param>
/// <param name="DeadLetterQueues">Tổng hợp Hàng đợi thư chết.</param>
/// <param name="Health">Tổng hợp Kiểm tra sức khỏe.</param>
public sealed record DashboardSummaryResponse(
    NotificationStatusSummaryDto Notifications,
    IReadOnlyList<DeadLetterQueueSummaryDto> DeadLetterQueues,
    HealthSummaryDto Health);