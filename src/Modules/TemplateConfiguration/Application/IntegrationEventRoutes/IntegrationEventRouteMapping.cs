using TemplateConfiguration.Domain.IntegrationEventRoutes;

namespace TemplateConfiguration.Application.IntegrationEventRoutes;

/// <summary>
/// Cung cấp hàm ánh xạ cho Định tuyến sự kiện tích hợp.
/// </summary>
internal static class IntegrationEventRouteMapping
{
    /// <summary>
    /// Ánh xạ thực thể Định tuyến sự kiện tích hợp sang DTO.
    /// </summary>
    /// <param name="route">Thực thể định tuyến.</param>
    /// <returns>DTO định tuyến.</returns>
    public static IntegrationEventRouteDto ToDto(IntegrationEventRoute route) =>
        new(
            route.Id,
            route.EventType,
            route.NotificationTemplateId,
            route.Channel,
            route.Description,
            route.IsActive,
            route.CreatedAtUtc,
            route.UpdatedAtUtc);
}
