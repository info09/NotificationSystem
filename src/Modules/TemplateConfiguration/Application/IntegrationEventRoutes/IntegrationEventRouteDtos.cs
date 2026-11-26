namespace TemplateConfiguration.Application.IntegrationEventRoutes;

/// <summary>
/// DTO thông tin Định tuyến sự kiện tích hợp.
/// </summary>
/// <param name="Id">Mã định danh định tuyến.</param>
/// <param name="EventType">Loại sự kiện tích hợp.</param>
/// <param name="NotificationTemplateId">Mã định danh Mẫu thông báo.</param>
/// <param name="Channel">Kênh thông báo.</param>
/// <param name="Description">Mô tả.</param>
/// <param name="IsActive">Trạng thái kích hoạt.</param>
/// <param name="CreatedAtUtc">Thời điểm tạo.</param>
/// <param name="UpdatedAtUtc">Thời điểm cập nhật.</param>
public sealed record IntegrationEventRouteDto(
    Guid Id,
    string EventType,
    Guid? NotificationTemplateId,
    string? Channel,
    string? Description,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

/// <summary>
/// Yêu cầu tạo Định tuyến sự kiện tích hợp.
/// </summary>
/// <param name="EventType">Loại sự kiện tích hợp.</param>
/// <param name="NotificationTemplateId">Mã định danh Mẫu thông báo.</param>
/// <param name="Channel">Kênh thông báo.</param>
/// <param name="Description">Mô tả.</param>
/// <param name="IsActive">Trạng thái kích hoạt.</param>
public sealed record CreateIntegrationEventRouteAppRequest(
    string? EventType,
    Guid? NotificationTemplateId,
    string? Channel,
    string? Description,
    bool? IsActive);

/// <summary>
/// Yêu cầu cập nhật Định tuyến sự kiện tích hợp.
/// </summary>
/// <param name="EventType">Loại sự kiện tích hợp.</param>
/// <param name="NotificationTemplateId">Mã định danh Mẫu thông báo.</param>
/// <param name="Channel">Kênh thông báo.</param>
/// <param name="Description">Mô tả.</param>
/// <param name="IsActive">Trạng thái kích hoạt.</param>
public sealed record UpdateIntegrationEventRouteAppRequest(
    string? EventType,
    Guid? NotificationTemplateId,
    string? Channel,
    string? Description,
    bool? IsActive);

/// <summary>
/// Yêu cầu truy vấn danh sách Định tuyến sự kiện tích hợp.
/// </summary>
/// <param name="EventType">Loại sự kiện lọc.</param>
/// <param name="Channel">Kênh lọc.</param>
/// <param name="IsActive">Trạng thái kích hoạt lọc.</param>
/// <param name="Page">Số trang.</param>
/// <param name="PageSize">Kích thước trang.</param>
public sealed record GetIntegrationEventRoutesAppRequest(
    string? EventType,
    string? Channel,
    bool? IsActive,
    int Page,
    int PageSize);

/// <summary>
/// Kết quả truy vấn danh sách Định tuyến sự kiện tích hợp.
/// </summary>
/// <param name="Items">Danh sách định tuyến.</param>
/// <param name="Page">Số trang hiện tại.</param>
/// <param name="PageSize">Kích thước trang.</param>
/// <param name="TotalCount">Tổng số bản ghi.</param>
public sealed record GetIntegrationEventRoutesResponse(
    IReadOnlyList<IntegrationEventRouteDto> Items,
    int Page,
    int PageSize,
    int TotalCount);
