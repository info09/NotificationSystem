namespace Delivery.Application.Email;

/// <summary>
/// Yêu cầu gửi Email.
/// </summary>
/// <param name="To">Địa chỉ người nhận.</param>
/// <param name="Subject">Tiêu đề Email.</param>
/// <param name="Content">Nội dung thuần.</param>
/// <param name="HtmlContent">Nội dung HTML.</param>
/// <param name="TemplateId">Mã Mẫu thông báo.</param>
/// <param name="TemplateData">Dữ liệu render mẫu.</param>
public sealed record SendEmailRequest(
    string To,
    string? Subject,
    string? Content,
    string? HtmlContent,
    Guid? TemplateId,
    IReadOnlyDictionary<string, string>? TemplateData);
