namespace TemplateConfiguration.Application.NotificationChannelConfigs;

/// <summary>
/// DTO thông tin Cấu hình kênh thông báo.
/// </summary>
/// <param name="Id">Mã định danh cấu hình.</param>
/// <param name="Channel">Kênh thông báo.</param>
/// <param name="DisplayName">Tên hiển thị.</param>
/// <param name="Description">Mô tả.</param>
/// <param name="IsEnabled">Trạng thái bật/tắt.</param>
/// <param name="CreatedAtUtc">Thời điểm tạo.</param>
/// <param name="UpdatedAtUtc">Thời điểm cập nhật.</param>
public sealed record NotificationChannelConfigDto(
    Guid Id,
    string Channel,
    string DisplayName,
    string? Description,
    bool IsEnabled,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

/// <summary>
/// Yêu cầu tạo Cấu hình kênh thông báo.
/// </summary>
/// <param name="Channel">Kênh thông báo.</param>
/// <param name="DisplayName">Tên hiển thị.</param>
/// <param name="Description">Mô tả.</param>
/// <param name="IsEnabled">Trạng thái bật/tắt.</param>
public sealed record CreateNotificationChannelConfigAppRequest(
    string? Channel,
    string? DisplayName,
    string? Description,
    bool? IsEnabled);

/// <summary>
/// Yêu cầu cập nhật Cấu hình kênh thông báo.
/// </summary>
/// <param name="DisplayName">Tên hiển thị.</param>
/// <param name="Description">Mô tả.</param>
/// <param name="IsEnabled">Trạng thái bật/tắt.</param>
public sealed record UpdateNotificationChannelConfigAppRequest(
    string? DisplayName,
    string? Description,
    bool? IsEnabled);

/// <summary>
/// Yêu cầu truy vấn danh sách Cấu hình kênh thông báo.
/// </summary>
/// <param name="Channel">Kênh lọc.</param>
/// <param name="IsEnabled">Trạng thái bật/tắt lọc.</param>
/// <param name="Page">Số trang.</param>
/// <param name="PageSize">Kích thước trang.</param>
public sealed record GetNotificationChannelConfigsAppRequest(
    string? Channel,
    bool? IsEnabled,
    int Page,
    int PageSize);

/// <summary>
/// Kết quả truy vấn danh sách Cấu hình kênh thông báo.
/// </summary>
/// <param name="Items">Danh sách cấu hình.</param>
/// <param name="Page">Số trang hiện tại.</param>
/// <param name="PageSize">Kích thước trang.</param>
/// <param name="TotalCount">Tổng số bản ghi.</param>
public sealed record GetNotificationChannelConfigsResponse(
    IReadOnlyList<NotificationChannelConfigDto> Items,
    int Page,
    int PageSize,
    int TotalCount);
