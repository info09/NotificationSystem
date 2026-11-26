namespace TemplateConfiguration.Application.NotificationTemplates;

/// <summary>
/// DTO thông tin Mẫu thông báo.
/// </summary>
/// <param name="Id">Mã định danh mẫu.</param>
/// <param name="TemplateKey">Khóa định danh nghiệp vụ.</param>
/// <param name="Name">Tên hiển thị.</param>
/// <param name="Description">Mô tả.</param>
/// <param name="Channel">Kênh thông báo.</param>
/// <param name="Subject">Tiêu đề.</param>
/// <param name="Body">Nội dung mẫu.</param>
/// <param name="IsActive">Trạng thái kích hoạt.</param>
/// <param name="CreatedAtUtc">Thời điểm tạo.</param>
/// <param name="UpdatedAtUtc">Thời điểm cập nhật.</param>
public sealed record NotificationTemplateDto(
    Guid Id,
    string TemplateKey,
    string Name,
    string? Description,
    string Channel,
    string? Subject,
    string Body,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

/// <summary>
/// Yêu cầu tạo Mẫu thông báo.
/// </summary>
/// <param name="TemplateKey">Khóa định danh nghiệp vụ.</param>
/// <param name="Name">Tên hiển thị.</param>
/// <param name="Description">Mô tả.</param>
/// <param name="Channel">Kênh thông báo.</param>
/// <param name="Subject">Tiêu đề.</param>
/// <param name="Body">Nội dung mẫu.</param>
/// <param name="IsActive">Trạng thái kích hoạt.</param>
public sealed record CreateNotificationTemplateAppRequest(
    string? TemplateKey,
    string? Name,
    string? Description,
    string? Channel,
    string? Subject,
    string? Body,
    bool? IsActive);

/// <summary>
/// Yêu cầu cập nhật Mẫu thông báo.
/// </summary>
/// <param name="Name">Tên hiển thị.</param>
/// <param name="Description">Mô tả.</param>
/// <param name="Channel">Kênh thông báo.</param>
/// <param name="Subject">Tiêu đề.</param>
/// <param name="Body">Nội dung mẫu.</param>
/// <param name="IsActive">Trạng thái kích hoạt.</param>
public sealed record UpdateNotificationTemplateAppRequest(
    string? Name,
    string? Description,
    string? Channel,
    string? Subject,
    string? Body,
    bool? IsActive);

/// <summary>
/// Yêu cầu truy vấn danh sách Mẫu thông báo.
/// </summary>
/// <param name="Channel">Kênh lọc.</param>
/// <param name="IsActive">Trạng thái kích hoạt lọc.</param>
/// <param name="Keyword">Từ khóa tìm kiếm.</param>
/// <param name="Page">Số trang.</param>
/// <param name="PageSize">Kích thước trang.</param>
public sealed record GetNotificationTemplatesAppRequest(
    string? Channel,
    bool? IsActive,
    string? Keyword,
    int Page,
    int PageSize);

/// <summary>
/// Kết quả truy vấn danh sách Mẫu thông báo.
/// </summary>
/// <param name="Items">Danh sách mẫu.</param>
/// <param name="Page">Số trang hiện tại.</param>
/// <param name="PageSize">Kích thước trang.</param>
/// <param name="TotalCount">Tổng số bản ghi.</param>
public sealed record GetNotificationTemplatesResponse(
    IReadOnlyList<NotificationTemplateDto> Items,
    int Page,
    int PageSize,
    int TotalCount);
