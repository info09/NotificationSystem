using BuildingBlocks.SharedKernel.Entity;

namespace IntegrationMessaging.Domain;

/// <summary>
/// Bản ghi Sự kiện tích hợp đã tiếp nhận để ghi nhận truy vết và phục vụ xử lý idempotent.
/// </summary>
public sealed class IntegrationEvent : Entity
{
    /// <summary>
    /// Khởi tạo bản ghi Sự kiện tích hợp cho ORM.
    /// </summary>
    private IntegrationEvent()
    {
        EventType = string.Empty;
        Source = string.Empty;
        Version = string.Empty;
        Payload = string.Empty;
    }

    private IntegrationEvent(
        Guid id,
        Guid messageId,
        Guid eventId,
        string eventType,
        DateTime occurredAtUtc,
        string source,
        string? idempotencyKey,
        string version,
        string payload,
        DateTime receivedAtUtc)
    {
        Id = id;
        MessageId = messageId;
        EventId = eventId;
        EventType = eventType;
        OccurredAtUtc = occurredAtUtc;
        Source = source;
        IdempotencyKey = idempotencyKey;
        Version = version;
        Payload = payload;
        ReceivedAtUtc = receivedAtUtc;
    }

    /// <summary>
    /// Mã định danh Thông điệp dùng để chống trùng xử lý.
    /// </summary>
    public Guid MessageId { get; private set; }

    /// <summary>
    /// Mã định danh Sự kiện tích hợp.
    /// </summary>
    public Guid EventId { get; private set; }

    /// <summary>
    /// Loại Sự kiện tích hợp.
    /// </summary>
    public string EventType { get; private set; }

    /// <summary>
    /// Thời điểm phát sinh sự kiện (UTC).
    /// </summary>
    public DateTime OccurredAtUtc { get; private set; }

    /// <summary>
    /// Hệ thống nguồn phát sinh sự kiện.
    /// </summary>
    public string Source { get; private set; }

    /// <summary>
    /// Khóa chống trùng do nguồn cung cấp.
    /// </summary>
    public string? IdempotencyKey { get; private set; }

    /// <summary>
    /// Phiên bản envelope sự kiện.
    /// </summary>
    public string Version { get; private set; }

    /// <summary>
    /// Nội dung payload JSON của sự kiện.
    /// </summary>
    public string Payload { get; private set; }

    /// <summary>
    /// Thời điểm hệ thống tiếp nhận sự kiện (UTC).
    /// </summary>
    public DateTime ReceivedAtUtc { get; private set; }

    /// <summary>
    /// Tạo bản ghi Sự kiện tích hợp đã tiếp nhận.
    /// </summary>
    /// <param name="id">Mã định danh bản ghi.</param>
    /// <param name="messageId">Mã định danh Thông điệp.</param>
    /// <param name="eventId">Mã định danh Sự kiện tích hợp.</param>
    /// <param name="eventType">Loại Sự kiện tích hợp.</param>
    /// <param name="occurredAtUtc">Thời điểm phát sinh sự kiện.</param>
    /// <param name="source">Hệ thống nguồn.</param>
    /// <param name="idempotencyKey">Khóa chống trùng.</param>
    /// <param name="version">Phiên bản envelope.</param>
    /// <param name="payload">Nội dung payload JSON.</param>
    /// <param name="receivedAtUtc">Thời điểm tiếp nhận.</param>
    /// <returns>Bản ghi Sự kiện tích hợp.</returns>
    public static IntegrationEvent Create(
        Guid id,
        Guid messageId,
        Guid eventId,
        string eventType,
        DateTime occurredAtUtc,
        string source,
        string? idempotencyKey,
        string version,
        string payload,
        DateTime receivedAtUtc)
    {
        if (messageId == Guid.Empty)
        {
            throw new ArgumentException("MessageId is required.", nameof(messageId));
        }

        if (eventId == Guid.Empty)
        {
            throw new ArgumentException("EventId is required.", nameof(eventId));
        }

        if (string.IsNullOrWhiteSpace(eventType))
        {
            throw new ArgumentException("EventType is required.", nameof(eventType));
        }

        if (string.IsNullOrWhiteSpace(source))
        {
            throw new ArgumentException("Source is required.", nameof(source));
        }

        if (string.IsNullOrWhiteSpace(version))
        {
            throw new ArgumentException("Version is required.", nameof(version));
        }

        if (string.IsNullOrWhiteSpace(payload))
        {
            throw new ArgumentException("Payload is required.", nameof(payload));
        }

        return new IntegrationEvent(
            id,
            messageId,
            eventId,
            eventType.Trim(),
            occurredAtUtc,
            source.Trim(),
            string.IsNullOrWhiteSpace(idempotencyKey) ? null : idempotencyKey.Trim(),
            version.Trim(),
            payload,
            receivedAtUtc);
    }
}
