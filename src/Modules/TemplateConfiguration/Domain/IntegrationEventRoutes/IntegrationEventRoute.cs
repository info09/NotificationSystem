using BuildingBlocks.SharedKernel.Entity;

namespace TemplateConfiguration.Domain.IntegrationEventRoutes;

/// <summary>
/// Thực thể Định tuyến sự kiện tích hợp.
/// </summary>
public sealed class IntegrationEventRoute : Entity
{
    /// <summary>
    /// Khởi tạo thực thể Định tuyến sự kiện tích hợp cho ORM.
    /// </summary>
    private IntegrationEventRoute()
    {
        EventType = string.Empty;
    }

    private IntegrationEventRoute(
        Guid id,
        string eventType,
        Guid? notificationTemplateId,
        string? channel,
        string? description,
        bool isActive,
        DateTime createdAtUtc,
        DateTime updatedAtUtc)
    {
        Id = id;
        EventType = eventType;
        NotificationTemplateId = notificationTemplateId;
        Channel = channel;
        Description = description;
        IsActive = isActive;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
    }

    /// <summary>
    /// Loại sự kiện tích hợp.
    /// </summary>
    public string EventType { get; private set; }

    /// <summary>
    /// Mã định danh Mẫu thông báo được ánh xạ.
    /// </summary>
    public Guid? NotificationTemplateId { get; private set; }

    /// <summary>
    /// Kênh thông báo được ánh xạ.
    /// </summary>
    public string? Channel { get; private set; }

    /// <summary>
    /// Mô tả định tuyến.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Trạng thái kích hoạt.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Thời điểm tạo (UTC).
    /// </summary>
    public DateTime CreatedAtUtc { get; private set; }

    /// <summary>
    /// Thời điểm cập nhật gần nhất (UTC).
    /// </summary>
    public DateTime UpdatedAtUtc { get; private set; }

    /// <summary>
    /// Tạo mới Định tuyến sự kiện tích hợp.
    /// </summary>
    /// <param name="id">Mã định danh.</param>
    /// <param name="eventType">Loại sự kiện tích hợp.</param>
    /// <param name="notificationTemplateId">Mã định danh Mẫu thông báo.</param>
    /// <param name="channel">Kênh thông báo.</param>
    /// <param name="description">Mô tả.</param>
    /// <param name="isActive">Trạng thái kích hoạt.</param>
    /// <param name="createdAtUtc">Thời điểm tạo.</param>
    /// <returns>Thực thể Định tuyến sự kiện tích hợp.</returns>
    public static IntegrationEventRoute Create(
        Guid id,
        string eventType,
        Guid? notificationTemplateId,
        string? channel,
        string? description,
        bool isActive = true,
        DateTime? createdAtUtc = null)
    {
        if (string.IsNullOrWhiteSpace(eventType))
        {
            throw new ArgumentException("Event type is required.", nameof(eventType));
        }

        if (notificationTemplateId is null && string.IsNullOrWhiteSpace(channel))
        {
            throw new ArgumentException("NotificationTemplateId or Channel is required.", nameof(notificationTemplateId));
        }

        var now = createdAtUtc ?? DateTime.UtcNow;
        return new IntegrationEventRoute(
            id,
            eventType.Trim(),
            notificationTemplateId,
            NormalizeOptional(channel),
            NormalizeOptional(description),
            isActive,
            now,
            now);
    }

    /// <summary>
    /// Cập nhật Định tuyến sự kiện tích hợp.
    /// </summary>
    /// <param name="eventType">Loại sự kiện tích hợp.</param>
    /// <param name="notificationTemplateId">Mã định danh Mẫu thông báo.</param>
    /// <param name="channel">Kênh thông báo.</param>
    /// <param name="description">Mô tả.</param>
    /// <param name="isActive">Trạng thái kích hoạt.</param>
    /// <param name="updatedAtUtc">Thời điểm cập nhật.</param>
    public void Update(
        string eventType,
        Guid? notificationTemplateId,
        string? channel,
        string? description,
        bool isActive,
        DateTime? updatedAtUtc = null)
    {
        if (string.IsNullOrWhiteSpace(eventType))
        {
            throw new ArgumentException("Event type is required.", nameof(eventType));
        }

        if (notificationTemplateId is null && string.IsNullOrWhiteSpace(channel))
        {
            throw new ArgumentException("NotificationTemplateId or Channel is required.", nameof(notificationTemplateId));
        }

        EventType = eventType.Trim();
        NotificationTemplateId = notificationTemplateId;
        Channel = NormalizeOptional(channel);
        Description = NormalizeOptional(description);
        IsActive = isActive;
        UpdatedAtUtc = updatedAtUtc ?? DateTime.UtcNow;
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
