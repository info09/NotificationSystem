namespace Notification.Application.Notifications.CreateNotification;

/// <summary>
/// Yêu cầu tạo Thông báo ở tầng Application.
/// </summary>
/// <param name="IdempotencyKey">Khóa idempotency của yêu cầu.</param>
/// <param name="Recipient">Người nhận Thông báo.</param>
/// <param name="Channel">Kênh gửi Thông báo.</param>
/// <param name="Content">Nội dung Thông báo.</param>
/// <param name="NotificationTemplateId">Mã Mẫu thông báo liên kết.</param>
/// <param name="Caller">Ngữ cảnh người gọi.</param>
public sealed record CreateNotificationAppRequest(
    string? IdempotencyKey,
    string? Recipient,
    string? Channel,
    string? Content,
    Guid? NotificationTemplateId,
    NotificationCallerContext Caller);