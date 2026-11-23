using BuildingBlocks.SharedKernel.Entity;

namespace Delivery.Domain.DeliveryLogs;

/// <summary>
/// Thực thể Nhật ký gửi thông báo.
/// </summary>
public sealed class NotificationDeliveryLog : Entity
{
    /// <summary>
    /// Khởi tạo thực thể Nhật ký gửi thông báo cho ORM.
    /// </summary>
    private NotificationDeliveryLog()
    {
        Channel = string.Empty;
    }

    private NotificationDeliveryLog(
        Guid id,
        Guid notificationId,
        string channel,
        DeliveryAttemptResult result,
        string? errorMessage,
        int retryCount,
        string? correlationId,
        DateTime createdAtUtc)
    {
        Id = id;
        NotificationId = notificationId;
        Channel = channel;
        Result = result;
        ErrorMessage = errorMessage;
        RetryCount = retryCount;
        CorrelationId = correlationId;
        CreatedAtUtc = createdAtUtc;
    }

    /// <summary>
    /// Mã định danh Thông báo.
    /// </summary>
    public Guid NotificationId { get; private set; }

    /// <summary>
    /// Kênh gửi.
    /// </summary>
    public string Channel { get; private set; }

    /// <summary>
    /// Kết quả gửi gần nhất.
    /// </summary>
    public DeliveryAttemptResult Result { get; private set; }

    /// <summary>
    /// Thông tin lỗi gần nhất.
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// Số lần thử lại.
    /// </summary>
    public int RetryCount { get; private set; }

    /// <summary>
    /// Mã tương quan để truy vết.
    /// </summary>
    public string? CorrelationId { get; private set; }

    /// <summary>
    /// Thời điểm tạo nhật ký (UTC).
    /// </summary>
    public DateTime CreatedAtUtc { get; private set; }

    /// <summary>
    /// Tạo mới Nhật ký gửi thông báo.
    /// </summary>
    /// <param name="id">Mã định danh Nhật ký gửi thông báo.</param>
    /// <param name="notificationId">Mã định danh Thông báo.</param>
    /// <param name="channel">Kênh gửi.</param>
    /// <param name="result">Kết quả gửi.</param>
    /// <param name="errorMessage">Thông tin lỗi.</param>
    /// <param name="retryCount">Số lần thử lại.</param>
    /// <param name="correlationId">Mã tương quan.</param>
    /// <param name="createdAtUtc">Thời điểm tạo nhật ký.</param>
    /// <returns>Thực thể Nhật ký gửi thông báo.</returns>
    public static NotificationDeliveryLog Create(
        Guid id,
        Guid notificationId,
        string channel,
        DeliveryAttemptResult result,
        string? errorMessage,
        int retryCount,
        string? correlationId,
        DateTime? createdAtUtc = null)
    {
        if (notificationId == Guid.Empty)
        {
            throw new ArgumentException("NotificationId is required.", nameof(notificationId));
        }

        if (string.IsNullOrWhiteSpace(channel))
        {
            throw new ArgumentException("Channel is required.", nameof(channel));
        }

        if (retryCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(retryCount), "Retry count must be greater than or equal to 0.");
        }

        return new NotificationDeliveryLog(
            id,
            notificationId,
            channel.Trim(),
            result,
            string.IsNullOrWhiteSpace(errorMessage) ? null : errorMessage.Trim(),
            retryCount,
            string.IsNullOrWhiteSpace(correlationId) ? null : correlationId.Trim(),
            createdAtUtc ?? DateTime.UtcNow);
    }
}
