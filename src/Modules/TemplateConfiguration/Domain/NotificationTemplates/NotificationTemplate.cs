using BuildingBlocks.SharedKernel.Entity;

namespace TemplateConfiguration.Domain.NotificationTemplates;

/// <summary>
/// Thực thể Mẫu thông báo.
/// </summary>
public sealed class NotificationTemplate : Entity
{
    /// <summary>
    /// Khởi tạo thực thể Mẫu thông báo cho ORM.
    /// </summary>
    private NotificationTemplate()
    {
        TemplateKey = string.Empty;
        Name = string.Empty;
        Channel = string.Empty;
        Body = string.Empty;
    }

    private NotificationTemplate(
        Guid id,
        string templateKey,
        string name,
        string? description,
        string channel,
        string? subject,
        string body,
        bool isActive,
        DateTime createdAtUtc,
        DateTime updatedAtUtc)
    {
        Id = id;
        TemplateKey = templateKey;
        Name = name;
        Description = description;
        Channel = channel;
        Subject = subject;
        Body = body;
        IsActive = isActive;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
    }

    /// <summary>
    /// Khóa định danh nghiệp vụ của Mẫu thông báo.
    /// </summary>
    public string TemplateKey { get; private set; }

    /// <summary>
    /// Tên hiển thị của Mẫu thông báo.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Mô tả Mẫu thông báo.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Kênh thông báo áp dụng cho mẫu.
    /// </summary>
    public string Channel { get; private set; }

    /// <summary>
    /// Tiêu đề mẫu.
    /// </summary>
    public string? Subject { get; private set; }

    /// <summary>
    /// Nội dung mẫu.
    /// </summary>
    public string Body { get; private set; }

    /// <summary>
    /// Trạng thái kích hoạt của mẫu.
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
    /// Tạo mới Mẫu thông báo.
    /// </summary>
    /// <param name="id">Mã định danh mẫu.</param>
    /// <param name="templateKey">Khóa định danh nghiệp vụ.</param>
    /// <param name="name">Tên hiển thị.</param>
    /// <param name="description">Mô tả.</param>
    /// <param name="channel">Kênh thông báo.</param>
    /// <param name="subject">Tiêu đề.</param>
    /// <param name="body">Nội dung mẫu.</param>
    /// <param name="isActive">Trạng thái kích hoạt.</param>
    /// <param name="createdAtUtc">Thời điểm tạo.</param>
    /// <returns>Thực thể Mẫu thông báo.</returns>
    public static NotificationTemplate Create(
        Guid id,
        string templateKey,
        string name,
        string? description,
        string channel,
        string? subject,
        string body,
        bool isActive = true,
        DateTime? createdAtUtc = null)
    {
        if (string.IsNullOrWhiteSpace(templateKey))
        {
            throw new ArgumentException("Template key is required.", nameof(templateKey));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(channel))
        {
            throw new ArgumentException("Channel is required.", nameof(channel));
        }

        if (string.IsNullOrWhiteSpace(body))
        {
            throw new ArgumentException("Body is required.", nameof(body));
        }

        var now = createdAtUtc ?? DateTime.UtcNow;
        return new NotificationTemplate(
            id,
            templateKey.Trim(),
            name.Trim(),
            NormalizeOptional(description),
            channel.Trim(),
            NormalizeOptional(subject),
            body.Trim(),
            isActive,
            now,
            now);
    }

    /// <summary>
    /// Cập nhật Mẫu thông báo.
    /// </summary>
    /// <param name="name">Tên hiển thị.</param>
    /// <param name="description">Mô tả.</param>
    /// <param name="channel">Kênh thông báo.</param>
    /// <param name="subject">Tiêu đề.</param>
    /// <param name="body">Nội dung mẫu.</param>
    /// <param name="isActive">Trạng thái kích hoạt.</param>
    /// <param name="updatedAtUtc">Thời điểm cập nhật.</param>
    public void Update(
        string name,
        string? description,
        string channel,
        string? subject,
        string body,
        bool isActive,
        DateTime? updatedAtUtc = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(channel))
        {
            throw new ArgumentException("Channel is required.", nameof(channel));
        }

        if (string.IsNullOrWhiteSpace(body))
        {
            throw new ArgumentException("Body is required.", nameof(body));
        }

        Name = name.Trim();
        Description = NormalizeOptional(description);
        Channel = channel.Trim();
        Subject = NormalizeOptional(subject);
        Body = body.Trim();
        IsActive = isActive;
        UpdatedAtUtc = updatedAtUtc ?? DateTime.UtcNow;
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
