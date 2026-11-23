namespace Delivery.Application.InAppNotifications;

/// <summary>
/// DTO phản hồi thông tin Thông báo trong ứng dụng.
/// </summary>
/// <param name="Id">Mã định danh Thông báo trong ứng dụng.</param>
/// <param name="NotificationId">Mã định danh Thông báo gốc.</param>
/// <param name="RecipientUserId">Mã định danh Người dùng cuối nhận Thông báo.</param>
/// <param name="Title">Tiêu đề Thông báo trong ứng dụng.</param>
/// <param name="Content">Nội dung Thông báo trong ứng dụng.</param>
/// <param name="ReadStatus">Trạng thái đọc.</param>
/// <param name="CreatedAtUtc">Thời điểm tạo (UTC).</param>
/// <param name="ReadAtUtc">Thời điểm đọc (UTC).</param>
public sealed record InAppNotificationResponseDto(
    Guid Id,
    Guid NotificationId,
    string RecipientUserId,
    string? Title,
    string? Content,
    string ReadStatus,
    DateTime CreatedAtUtc,
    DateTime? ReadAtUtc);

/// <summary>
/// DTO phản hồi danh sách Thông báo trong ứng dụng có phân trang.
/// </summary>
/// <param name="Items">Danh sách Thông báo trong ứng dụng.</param>
/// <param name="TotalCount">Tổng số bản ghi.</param>
/// <param name="Page">Số trang hiện tại.</param>
/// <param name="PageSize">Kích thước trang.</param>
public sealed record GetInAppNotificationsResponseDto(
    IReadOnlyList<InAppNotificationResponseDto> Items,
    int TotalCount,
    int Page,
    int PageSize);

/// <summary>
/// DTO phản hồi số lượng Thông báo trong ứng dụng chưa đọc.
/// </summary>
/// <param name="UnreadCount">Số lượng Thông báo chưa đọc.</param>
public sealed record UnreadInAppNotificationCountResponseDto(int UnreadCount);
