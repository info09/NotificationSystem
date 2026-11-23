using BuildingBlocks.SharedKernel.Entity;

namespace IntegrationMessaging.Domain;

/// <summary>
/// Bản ghi Inbox lưu dấu vết Thông điệp đã xử lý để bảo đảm Idempotency.
/// </summary>
public sealed class InboxMessage : Entity
{
    /// <summary>
    /// Khởi tạo bản ghi Inbox cho ORM.
    /// </summary>
    private InboxMessage()
    {
        ConsumerName = string.Empty;
        EventType = string.Empty;
    }

    private InboxMessage(
        Guid id,
        Guid messageId,
        string consumerName,
        string eventType,
        DateTime processedAtUtc)
    {
        Id = id;
        MessageId = messageId;
        ConsumerName = consumerName;
        EventType = eventType;
        ProcessedAtUtc = processedAtUtc;
    }

    /// <summary>
    /// Mã định danh Thông điệp dùng để chống trùng xử lý.
    /// </summary>
    public Guid MessageId { get; private set; }

    /// <summary>
    /// Tên consumer/ngữ cảnh xử lý để phân biệt phạm vi idempotency.
    /// </summary>
    public string ConsumerName { get; private set; }

    /// <summary>
    /// Loại sự kiện/thông điệp đã xử lý.
    /// </summary>
    public string EventType { get; private set; }

    /// <summary>
    /// Thời điểm xử lý thành công (UTC).
    /// </summary>
    public DateTime ProcessedAtUtc { get; private set; }

    /// <summary>
    /// Tạo bản ghi Inbox cho Thông điệp đã xử lý.
    /// </summary>
    /// <param name="id">Mã định danh bản ghi Inbox.</param>
    /// <param name="messageId">Mã định danh Thông điệp.</param>
    /// <param name="consumerName">Tên consumer/ngữ cảnh xử lý.</param>
    /// <param name="eventType">Loại sự kiện/thông điệp.</param>
    /// <param name="processedAtUtc">Thời điểm xử lý thành công (UTC).</param>
    /// <returns>Bản ghi Inbox mới.</returns>
    public static InboxMessage Create(
        Guid id,
        Guid messageId,
        string consumerName,
        string eventType,
        DateTime processedAtUtc)
    {
        if (messageId == Guid.Empty)
        {
            throw new ArgumentException("MessageId is required.", nameof(messageId));
        }

        if (string.IsNullOrWhiteSpace(consumerName))
        {
            throw new ArgumentException("Consumer name is required.", nameof(consumerName));
        }

        if (string.IsNullOrWhiteSpace(eventType))
        {
            throw new ArgumentException("Event type is required.", nameof(eventType));
        }

        return new InboxMessage(id, messageId, consumerName, eventType, processedAtUtc);
    }
}
