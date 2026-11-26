using TemplateConfiguration.Domain.NotificationChannelConfigs;

namespace TemplateConfiguration.Application.NotificationChannelConfigs;

/// <summary>
/// Cung cấp hàm ánh xạ cho Cấu hình kênh thông báo.
/// </summary>
internal static class NotificationChannelConfigMapping
{
    /// <summary>
    /// Ánh xạ thực thể Cấu hình kênh thông báo sang DTO.
    /// </summary>
    /// <param name="config">Thực thể cấu hình.</param>
    /// <returns>DTO cấu hình.</returns>
    public static NotificationChannelConfigDto ToDto(NotificationChannelConfig config) =>
        new(
            config.Id,
            config.Channel,
            config.DisplayName,
            config.Description,
            config.IsEnabled,
            config.CreatedAtUtc,
            config.UpdatedAtUtc);
}
