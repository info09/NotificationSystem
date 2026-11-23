namespace Delivery.Domain.DeliveryLogs;

/// <summary>
/// Kết quả của lần thử gửi Thông báo.
/// </summary>
public enum DeliveryAttemptResult
{
    /// <summary>
    /// Gửi thành công.
    /// </summary>
    Success = 1,

    /// <summary>
    /// Gửi thất bại.
    /// </summary>
    Failed = 2
}
