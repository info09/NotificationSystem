using TemplateConfiguration.Domain.NotificationTemplates;

namespace TemplateConfiguration.Application.NotificationTemplates;

/// <summary>
/// Cung cấp hàm ánh xạ cho Mẫu thông báo.
/// </summary>
internal static class NotificationTemplateMapping
{
    /// <summary>
    /// Ánh xạ thực thể Mẫu thông báo sang DTO.
    /// </summary>
    /// <param name="template">Thực thể mẫu thông báo.</param>
    /// <returns>DTO mẫu thông báo.</returns>
    public static NotificationTemplateDto ToDto(NotificationTemplate template) =>
        new(
            template.Id,
            template.TemplateKey,
            template.Name,
            template.Description,
            template.Channel,
            template.Subject,
            template.Body,
            template.IsActive,
            template.CreatedAtUtc,
            template.UpdatedAtUtc);
}
