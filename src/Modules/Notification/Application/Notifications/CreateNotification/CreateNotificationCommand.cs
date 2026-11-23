using NotificationChannel = Notification.Domain.NotificationChannel;

namespace Notification.Application.Notifications.CreateNotification;

/// <summary>
/// Lệnh tạo Thông báo đã được chuẩn hóa.
/// </summary>
/// <param name="Recipient">Người nhận Thông báo.</param>
/// <param name="Channel">Kênh gửi Thông báo.</param>
/// <param name="Content">Nội dung Thông báo.</param>
/// <param name="NotificationTemplateId">Mã Mẫu thông báo liên kết.</param>
/// <param name="IdempotencyScope">Phạm vi idempotency.</param>
/// <param name="IdempotencyKey">Khóa idempotency.</param>
public sealed record CreateNotificationCommand(
    string Recipient,
    NotificationChannel Channel,
    string? Content,
    Guid? NotificationTemplateId,
    string IdempotencyScope,
    string IdempotencyKey);
