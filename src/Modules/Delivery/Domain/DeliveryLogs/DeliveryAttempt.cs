using BuildingBlocks.SharedKernel.Entity;

namespace Delivery.Domain.DeliveryLogs;

/// <summary>
/// Thực thể Lần thử gửi Thông báo.
/// </summary>
public sealed class DeliveryAttempt : Entity
{
    /// <summary>
    /// Khởi tạo thực thể Lần thử gửi cho ORM.
    /// </summary>
    private DeliveryAttempt()
    {
        Channel = string.Empty;
    }

    private DeliveryAttempt(
        Guid id,
        Guid notificationId,
        string channel,
        int attemptNumber,
        DeliveryAttemptResult result,
        string? errorMessage,
        string? correlationId,
        DateTime attemptedAtUtc)
    {
        Id = id;
        NotificationId = notificationId;
        Channel = channel;
        AttemptNumber = attemptNumber;
        Result = result;
        ErrorMessage = errorMessage;
        CorrelationId = correlationId;
        AttemptedAtUtc = attemptedAtUtc;
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
    /// Số thứ tự lần thử gửi.
    /// </summary>
    public int AttemptNumber { get; private set; }

    /// <summary>
    /// Kết quả lần thử gửi.
    /// </summary>
    public DeliveryAttemptResult Result { get; private set; }

    /// <summary>
    /// Thông tin lỗi khi gửi thất bại.
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// Mã tương quan để truy vết.
    /// </summary>
    public string? CorrelationId { get; private set; }

    /// <summary>
    /// Thời điểm thử gửi (UTC).
    /// </summary>
    public DateTime AttemptedAtUtc { get; private set; }

    /// <summary>
    /// Tạo mới Lần thử gửi.
    /// </summary>
    /// <param name="id">Mã định danh Lần thử gửi.</param>
    /// <param name="notificationId">Mã định danh Thông báo.</param>
    /// <param name="channel">Kênh gửi.</param>
    /// <param name="attemptNumber">Số thứ tự lần thử gửi.</param>
    /// <param name="result">Kết quả lần thử gửi.</param>
    /// <param name="errorMessage">Thông tin lỗi.</param>
    /// <param name="correlationId">Mã tương quan.</param>
    /// <param name="attemptedAtUtc">Thời điểm thử gửi.</param>
    /// <returns>Thực thể Lần thử gửi.</returns>
    public static DeliveryAttempt Create(
        Guid id,
        Guid notificationId,
        string channel,
        int attemptNumber,
        DeliveryAttemptResult result,
        string? errorMessage,
        string? correlationId,
        DateTime? attemptedAtUtc = null)
    {
        if (notificationId == Guid.Empty)
        {
            throw new ArgumentException("NotificationId is required.", nameof(notificationId));
        }

        if (string.IsNullOrWhiteSpace(channel))
        {
            throw new ArgumentException("Channel is required.", nameof(channel));
        }

        if (attemptNumber < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(attemptNumber), "Attempt number must be greater than 0.");
        }

        return new DeliveryAttempt(
            id,
            notificationId,
            channel.Trim(),
            attemptNumber,
            result,
            string.IsNullOrWhiteSpace(errorMessage) ? null : errorMessage.Trim(),
            string.IsNullOrWhiteSpace(correlationId) ? null : correlationId.Trim(),
            attemptedAtUtc ?? DateTime.UtcNow);
    }
}
