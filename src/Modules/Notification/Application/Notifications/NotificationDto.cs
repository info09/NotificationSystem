using NotificationChannel = Notification.Domain.NotificationChannel;
using NotificationStatus = Notification.Domain.NotificationStatus;

namespace Notification.Application.Notifications;

/// <summary>
/// DTO biểu diễn dữ liệu Thông báo.
/// </summary>
/// <param name="Id">Mã định danh Thông báo.</param>
/// <param name="Recipient">Người nhận Thông báo.</param>
/// <param name="Channel">Kênh gửi Thông báo.</param>
/// <param name="Status">Trạng thái Thông báo.</param>
/// <param name="Content">Nội dung Thông báo.</param>
/// <param name="NotificationTemplateId">Mã Mẫu thông báo liên kết.</param>
/// <param name="CreatedAt">Thời điểm tạo Thông báo.</param>
public sealed record NotificationDto(
    Guid Id,
    string Recipient,
    NotificationChannel Channel,
    NotificationStatus Status,
    string? Content,
    Guid? NotificationTemplateId,
    DateTime CreatedAt);
