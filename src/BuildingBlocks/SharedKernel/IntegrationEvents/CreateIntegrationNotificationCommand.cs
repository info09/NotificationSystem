namespace BuildingBlocks.SharedKernel.IntegrationEvents;

/// <summary>
/// Lệnh tạo Thông báo từ Sự kiện tích hợp.
/// </summary>
/// <param name="Recipient">Người nhận thông báo.</param>
/// <param name="Channel">Kênh thông báo.</param>
/// <param name="Content">Nội dung thông báo.</param>
/// <param name="NotificationTemplateId">Mã định danh Mẫu thông báo.</param>
/// <param name="IdempotencyScope">Phạm vi chống trùng.</param>
/// <param name="IdempotencyKey">Khóa chống trùng.</param>
public sealed record CreateIntegrationNotificationCommand(
    string Recipient,
    string Channel,
    string? Content,
    Guid? NotificationTemplateId,
    string IdempotencyScope,
    string IdempotencyKey);
