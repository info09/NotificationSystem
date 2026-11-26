using BuildingBlocks.SharedKernel.Entity;

namespace TemplateConfiguration.Domain.NotificationChannelConfigs;

/// <summary>
/// Thực thể Cấu hình kênh thông báo.
/// </summary>
public sealed class NotificationChannelConfig : Entity
{
    /// <summary>
    /// Khởi tạo thực thể Cấu hình kênh thông báo cho ORM.
    /// </summary>
    private NotificationChannelConfig()
    {
        Channel = string.Empty;
        DisplayName = string.Empty;
    }

    private NotificationChannelConfig(
        Guid id,
        string channel,
        string displayName,
        string? description,
        bool isEnabled,
        DateTime createdAtUtc,
        DateTime updatedAtUtc)
    {
        Id = id;
        Channel = channel;
        DisplayName = displayName;
        Description = description;
        IsEnabled = isEnabled;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
    }

    /// <summary>
    /// Kênh thông báo được cấu hình.
    /// </summary>
    public string Channel { get; private set; }

    /// <summary>
    /// Tên hiển thị cấu hình.
    /// </summary>
    public string DisplayName { get; private set; }

    /// <summary>
    /// Mô tả cấu hình.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Trạng thái bật/tắt kênh.
    /// </summary>
    public bool IsEnabled { get; private set; }

    /// <summary>
    /// Thời điểm tạo (UTC).
    /// </summary>
    public DateTime CreatedAtUtc { get; private set; }

    /// <summary>
    /// Thời điểm cập nhật gần nhất (UTC).
    /// </summary>
    public DateTime UpdatedAtUtc { get; private set; }

    /// <summary>
    /// Tạo mới Cấu hình kênh thông báo.
    /// </summary>
    /// <param name="id">Mã định danh cấu hình.</param>
    /// <param name="channel">Kênh thông báo.</param>
    /// <param name="displayName">Tên hiển thị.</param>
    /// <param name="description">Mô tả.</param>
    /// <param name="isEnabled">Trạng thái bật/tắt.</param>
    /// <param name="createdAtUtc">Thời điểm tạo.</param>
    /// <returns>Thực thể Cấu hình kênh thông báo.</returns>
    public static NotificationChannelConfig Create(
        Guid id,
        string channel,
        string displayName,
        string? description,
        bool isEnabled = true,
        DateTime? createdAtUtc = null)
    {
        if (string.IsNullOrWhiteSpace(channel))
        {
            throw new ArgumentException("Channel is required.", nameof(channel));
        }

        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException("Display name is required.", nameof(displayName));
        }

        var now = createdAtUtc ?? DateTime.UtcNow;
        return new NotificationChannelConfig(
            id,
            channel.Trim(),
            displayName.Trim(),
            NormalizeOptional(description),
            isEnabled,
            now,
            now);
    }

    /// <summary>
    /// Cập nhật Cấu hình kênh thông báo.
    /// </summary>
    /// <param name="displayName">Tên hiển thị.</param>
    /// <param name="description">Mô tả.</param>
    /// <param name="isEnabled">Trạng thái bật/tắt.</param>
    /// <param name="updatedAtUtc">Thời điểm cập nhật.</param>
    public void Update(string displayName, string? description, bool isEnabled, DateTime? updatedAtUtc = null)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException("Display name is required.", nameof(displayName));
        }

        DisplayName = displayName.Trim();
        Description = NormalizeOptional(description);
        IsEnabled = isEnabled;
        UpdatedAtUtc = updatedAtUtc ?? DateTime.UtcNow;
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
