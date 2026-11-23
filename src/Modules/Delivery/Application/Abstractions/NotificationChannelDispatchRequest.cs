namespace Delivery.Application.Abstractions;

/// <summary>
/// Yêu cầu dispatch Thông báo theo kênh.
/// </summary>
/// <param name="NotificationId">Mã định danh Thông báo.</param>
/// <param name="Channel">Kênh gửi Thông báo.</param>
/// <param name="Recipient">Người nhận Thông báo.</param>
/// <param name="Content">Nội dung Thông báo.</param>
/// <param name="NotificationTemplateId">Mã Mẫu thông báo.</param>
public sealed record NotificationChannelDispatchRequest(
    Guid NotificationId,
    string Channel,
    string Recipient,
    string? Content,
    Guid? NotificationTemplateId);
