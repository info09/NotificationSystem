namespace BuildingBlocks.SharedKernel.IntegrationEvents;

/// <summary>
/// Kết quả phân giải Định tuyến sự kiện tích hợp.
/// </summary>
/// <param name="EventType">Loại sự kiện tích hợp.</param>
/// <param name="NotificationTemplateId">Mã định danh Mẫu thông báo.</param>
/// <param name="Channel">Kênh thông báo.</param>
public sealed record IntegrationEventRouteResolution(
    string EventType,
    Guid? NotificationTemplateId,
    string? Channel);
