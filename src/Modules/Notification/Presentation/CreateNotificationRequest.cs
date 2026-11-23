namespace Notification.Presentation;

/// <summary>
/// Request tạo Thông báo từ API.
/// </summary>
/// <param name="IdempotencyKey">Khóa idempotency của yêu cầu.</param>
/// <param name="Recipient">Người nhận Thông báo.</param>
/// <param name="Channel">Kênh gửi Thông báo.</param>
/// <param name="Content">Nội dung Thông báo.</param>
/// <param name="NotificationTemplateId">Mã Mẫu thông báo liên kết.</param>
public sealed record CreateNotificationRequest(
    string? IdempotencyKey,
    string? Recipient,
    string? Channel,
    string? Content,
    Guid? NotificationTemplateId);