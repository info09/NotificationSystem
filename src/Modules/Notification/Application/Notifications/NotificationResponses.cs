namespace Notification.Application.Notifications;

/// <summary>
/// DTO phản hồi khi tạo Thông báo.
/// </summary>
/// <param name="NotificationId">Mã định danh Thông báo.</param>
/// <param name="Status">Trạng thái Thông báo.</param>
public sealed record CreateNotificationResponseDto(Guid NotificationId, string Status);

/// <summary>
/// DTO phản hồi thông tin một Thông báo.
/// </summary>
/// <param name="Id">Mã định danh Thông báo.</param>
/// <param name="Recipient">Người nhận Thông báo.</param>
/// <param name="Channel">Kênh gửi Thông báo.</param>
/// <param name="Status">Trạng thái Thông báo.</param>
/// <param name="Content">Nội dung Thông báo.</param>
/// <param name="NotificationTemplateId">Mã Mẫu thông báo liên kết.</param>
/// <param name="CreatedAt">Thời điểm tạo Thông báo.</param>
public sealed record NotificationItemResponseDto(
    Guid Id,
    string Recipient,
    string Channel,
    string Status,
    string? Content,
    Guid? NotificationTemplateId,
    DateTime CreatedAt);

/// <summary>
/// DTO phản hồi danh sách Thông báo có phân trang.
/// </summary>
/// <param name="Items">Danh sách Thông báo.</param>
/// <param name="TotalCount">Tổng số bản ghi.</param>
/// <param name="Page">Số trang hiện tại.</param>
/// <param name="PageSize">Kích thước trang.</param>
public sealed record GetNotificationsResponseDto(
    IReadOnlyList<NotificationItemResponseDto> Items,
    int TotalCount,
    int Page,
    int PageSize);