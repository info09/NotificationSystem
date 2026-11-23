namespace Delivery.Application.Abstractions;

/// <summary>
/// Abstraction ghi nhận Nhật ký gửi thông báo và Lần thử gửi.
/// </summary>
public interface IDeliveryLogRecorder
{
    /// <summary>
    /// Ghi nhận một Lần thử gửi và cập nhật Nhật ký gửi thông báo.
    /// </summary>
    /// <param name="request">Yêu cầu ghi nhận Lần thử gửi.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    Task RecordAttemptAsync(RecordDeliveryAttemptRequest request, CancellationToken cancellationToken);
}
