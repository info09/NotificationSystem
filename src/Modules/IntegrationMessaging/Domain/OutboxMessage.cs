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
    /// <param name="id">Mã định danh bản ghi Outbox.</param>
    /// <param name="messageId">Mã định danh Thông điệp.</param>
    /// <param name="eventType">Loại sự kiện/thông điệp.</param>
    /// <param name="payload">Nội dung payload JSON.</param>
    /// <param name="status">Trạng thái publish.</param>
    /// <param name="occurredAtUtc">Thời điểm phát sinh sự kiện (UTC).</param>
    /// <param name="processedAtUtc">Thời điểm xử lý publish (UTC).</param>
    private OutboxMessage(
        Guid id,
        Guid messageId,
        string eventType,
        string payload,
        OutboxMessageStatus status,
        DateTime occurredAtUtc,
        DateTime? processedAtUtc)
    {
        Id = id;
        MessageId = messageId;
        EventType = eventType;
        Payload = payload;
        Status = status;
        OccurredAtUtc = occurredAtUtc;
        ProcessedAtUtc = processedAtUtc;
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
            processedAtUtc: null);
    }

    /// <summary>
    /// Đánh dấu bản ghi Outbox đã publish thành công.
    /// </summary>
    /// <param name="processedAtUtc">Thời điểm publish thành công (UTC).</param>
    public void MarkPublished(DateTime processedAtUtc)
    {
        Status = OutboxMessageStatus.Published;
        ProcessedAtUtc = processedAtUtc;
    }

    /// <summary>
    /// Đánh dấu bản ghi Outbox publish thất bại.
    /// </summary>
    public void MarkFailed()
    {
        Status = OutboxMessageStatus.Failed;
    }
}
