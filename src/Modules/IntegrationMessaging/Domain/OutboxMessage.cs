using BuildingBlocks.SharedKernel.Entity;

namespace IntegrationMessaging.Domain;

/// <summary>
/// Bản ghi Outbox đại diện cho Thông điệp cần publish theo Transactional Outbox.
/// </summary>
public sealed class OutboxMessage : Entity
{
    /// <summary>
    /// Khởi tạo bản ghi Outbox cho ORM.
    /// </summary>
    private OutboxMessage()
    {
        EventType = string.Empty;
        Payload = string.Empty;
    }

    /// <summary>
    /// Khởi tạo bản ghi Outbox với đầy đủ dữ liệu.
    /// </summary>
    private OutboxMessage(
        Guid id,
        Guid messageId,
        string eventType,
        string payload,
        OutboxMessageStatus status,
        DateTime occurredAtUtc,
        DateTime? processedAtUtc,
        int retryCount,
        DateTime nextAttemptAtUtc,
        string? lastError)
    {
        Id = id;
        MessageId = messageId;
        EventType = eventType;
        Payload = payload;
        Status = status;
        OccurredAtUtc = occurredAtUtc;
        ProcessedAtUtc = processedAtUtc;
        RetryCount = retryCount;
        NextAttemptAtUtc = nextAttemptAtUtc;
        LastError = lastError;
    }

    /// <summary>
    /// Mã định danh Thông điệp dùng cho truy vết và chống trùng.
    /// </summary>
    public Guid MessageId { get; private set; }

    /// <summary>
    /// Loại sự kiện/thông điệp cần publish.
    /// </summary>
    public string EventType { get; private set; }

    /// <summary>
    /// Nội dung payload JSON của Thông điệp.
    /// </summary>
    public string Payload { get; private set; }

    /// <summary>
    /// Trạng thái publish của bản ghi Outbox.
    /// </summary>
    public OutboxMessageStatus Status { get; private set; }

    /// <summary>
    /// Thời điểm phát sinh sự kiện (UTC).
    /// </summary>
    public DateTime OccurredAtUtc { get; private set; }

    /// <summary>
    /// Thời điểm publish thành công (UTC), null nếu chưa publish.
    /// </summary>
    public DateTime? ProcessedAtUtc { get; private set; }

    /// <summary>
    /// Số lần publish đã thử thất bại.
    /// </summary>
    public int RetryCount { get; private set; }

    /// <summary>
    /// Thời điểm được phép thử publish tiếp theo (UTC).
    /// </summary>
    public DateTime NextAttemptAtUtc { get; private set; }

    /// <summary>
    /// Lỗi publish gần nhất.
    /// </summary>
    public string? LastError { get; private set; }

    /// <summary>
    /// Tạo bản ghi Outbox ở trạng thái chờ publish.
    /// </summary>
    /// <param name="id">Mã định danh bản ghi Outbox.</param>
    /// <param name="messageId">Mã định danh Thông điệp.</param>
    /// <param name="eventType">Loại sự kiện/thông điệp.</param>
    /// <param name="payload">Nội dung payload JSON.</param>
    /// <param name="occurredAtUtc">Thời điểm phát sinh sự kiện (UTC).</param>
    /// <returns>Bản ghi Outbox mới.</returns>
    public static OutboxMessage CreatePending(
        Guid id,
        Guid messageId,
        string eventType,
        string payload,
        DateTime occurredAtUtc)
    {
        if (string.IsNullOrWhiteSpace(eventType))
        {
            throw new ArgumentException("Event type is required.", nameof(eventType));
        }

        if (string.IsNullOrWhiteSpace(payload))
        {
            throw new ArgumentException("Payload is required.", nameof(payload));
        }

        return new OutboxMessage(
            id,
            messageId,
            eventType,
            payload,
            OutboxMessageStatus.Pending,
            occurredAtUtc,
            processedAtUtc: null,
            retryCount: 0,
            nextAttemptAtUtc: occurredAtUtc,
            lastError: null);
    }

    /// <summary>
    /// Đánh dấu bản ghi Outbox đã publish thành công.
    /// </summary>
    /// <param name="processedAtUtc">Thời điểm publish thành công (UTC).</param>
    public void MarkPublished(DateTime processedAtUtc)
    {
        Status = OutboxMessageStatus.Published;
        ProcessedAtUtc = processedAtUtc;
        LastError = null;
    }

    /// <summary>
    /// Đánh dấu lần publish thất bại và lên lịch thử lại theo chính sách an toàn.
    /// </summary>
    /// <param name="error">Thông tin lỗi.</param>
    /// <param name="attemptedAtUtc">Thời điểm thử publish (UTC).</param>
    /// <param name="nextAttemptAtUtc">Thời điểm thử lại tiếp theo (UTC).</param>
    /// <param name="maxRetryCount">Số lần thử tối đa trước khi đánh dấu thất bại.</param>
    public void MarkPublishFailed(string error, DateTime attemptedAtUtc, DateTime nextAttemptAtUtc, int maxRetryCount)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(error);
        if (maxRetryCount < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maxRetryCount), "Max retry count must be at least 1.");
        }

        RetryCount++;
        LastError = error.Length > 2000 ? error[..2000] : error;
        ProcessedAtUtc = attemptedAtUtc;

        if (RetryCount >= maxRetryCount)
        {
            Status = OutboxMessageStatus.Failed;
            NextAttemptAtUtc = attemptedAtUtc;
            return;
        }

        Status = OutboxMessageStatus.Pending;
        NextAttemptAtUtc = nextAttemptAtUtc;
    }
}
