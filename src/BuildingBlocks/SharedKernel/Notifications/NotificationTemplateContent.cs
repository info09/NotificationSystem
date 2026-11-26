namespace BuildingBlocks.SharedKernel.Notifications;

/// <summary>
/// Nội dung Mẫu thông báo dùng cho render.
/// </summary>
/// <param name="TemplateId">Mã định danh Mẫu thông báo.</param>
/// <param name="Subject">Tiêu đề mẫu.</param>
/// <param name="Body">Nội dung mẫu.</param>
/// <param name="Channel">Kênh thông báo.</param>
/// <param name="IsActive">Trạng thái kích hoạt.</param>
public sealed record NotificationTemplateContent(
    Guid TemplateId,
    string? Subject,
    string Body,
    string Channel,
    bool IsActive);
