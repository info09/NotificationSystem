namespace Delivery.Application.DeliveryLogs;

/// <summary>
/// DTO Nhật ký gửi thông báo.
/// </summary>
/// <param name="Id">Mã định danh Nhật ký gửi thông báo.</param>
/// <param name="NotificationId">Mã định danh Thông báo.</param>
/// <param name="Channel">Kênh gửi.</param>
/// <param name="Result">Kết quả gửi.</param>
/// <param name="ErrorMessage">Thông tin lỗi.</param>
/// <param name="RetryCount">Số lần thử lại.</param>
/// <param name="CorrelationId">Mã tương quan.</param>
/// <param name="CreatedAtUtc">Thời điểm tạo.</param>
public sealed record DeliveryLogDto(
    Guid Id,
    Guid NotificationId,
    string Channel,
    string Result,
    string? ErrorMessage,
    int RetryCount,
    string? CorrelationId,
    DateTime CreatedAtUtc);

/// <summary>
/// DTO Lần thử gửi Thông báo.
/// </summary>
/// <param name="Id">Mã định danh Lần thử gửi.</param>
/// <param name="NotificationId">Mã định danh Thông báo.</param>
/// <param name="Channel">Kênh gửi.</param>
/// <param name="AttemptNumber">Số thứ tự lần thử gửi.</param>
/// <param name="Result">Kết quả lần thử gửi.</param>
/// <param name="ErrorMessage">Thông tin lỗi.</param>
/// <param name="CorrelationId">Mã tương quan.</param>
/// <param name="AttemptedAtUtc">Thời điểm thử gửi.</param>
public sealed record DeliveryAttemptDto(
    Guid Id,
    Guid NotificationId,
    string Channel,
    int AttemptNumber,
    string Result,
    string? ErrorMessage,
    string? CorrelationId,
    DateTime AttemptedAtUtc);

/// <summary>
/// DTO danh sách Lần thử gửi có phân trang.
/// </summary>
/// <param name="Items">Danh sách Lần thử gửi.</param>
/// <param name="TotalCount">Tổng số bản ghi.</param>
/// <param name="Page">Số trang.</param>
/// <param name="PageSize">Kích thước trang.</param>
public sealed record DeliveryAttemptsDto(
    IReadOnlyList<DeliveryAttemptDto> Items,
    int TotalCount,
    int Page,
    int PageSize);

/// <summary>
/// DTO phản hồi lịch sử gửi Thông báo.
/// </summary>
/// <param name="Logs">Danh sách Nhật ký gửi thông báo.</param>
/// <param name="Attempts">Danh sách Lần thử gửi có phân trang.</param>
public sealed record DeliveryHistoryResponseDto(
    IReadOnlyList<DeliveryLogDto> Logs,
    DeliveryAttemptsDto Attempts);
