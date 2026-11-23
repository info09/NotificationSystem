using Delivery.Domain.DeliveryLogs;

namespace Delivery.Application.Abstractions;

/// <summary>
/// Yêu cầu ghi nhận Lần thử gửi Thông báo.
/// </summary>
/// <param name="NotificationId">Mã định danh Thông báo.</param>
/// <param name="Channel">Kênh gửi.</param>
/// <param name="AttemptNumber">Số thứ tự lần thử gửi.</param>
/// <param name="Result">Kết quả lần thử gửi.</param>
/// <param name="ErrorMessage">Thông tin lỗi.</param>
/// <param name="CorrelationId">Mã tương quan.</param>
/// <param name="AttemptedAtUtc">Thời điểm thử gửi.</param>
public sealed record RecordDeliveryAttemptRequest(
    Guid NotificationId,
    string Channel,
    int AttemptNumber,
    DeliveryAttemptResult Result,
    string? ErrorMessage,
    string? CorrelationId,
    DateTime? AttemptedAtUtc = null);
