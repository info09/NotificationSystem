namespace Notification.Infrastructure.Persistence;

/// <summary>
/// Bản ghi Outbox dùng cho ghi dữ liệu trong ngữ cảnh Notification.
/// </summary>
internal sealed class OutboxMessageRecord
{
    /// <summary>
    /// Mã định danh bản ghi Outbox.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Mã định danh Thông điệp.
    /// </summary>
    public Guid MessageId { get; set; }

    /// <summary>
    /// Loại sự kiện/thông điệp.
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// Nội dung payload JSON.
    /// </summary>
    public string Payload { get; set; } = string.Empty;

    /// <summary>
    /// Trạng thái publish.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Thời điểm phát sinh sự kiện (UTC).
    /// </summary>
    public DateTime OccurredAtUtc { get; set; }

    /// <summary>
    /// Thời điểm publish thành công (UTC).
    /// </summary>
    public DateTime? ProcessedAtUtc { get; set; }
}
