namespace Delivery.Domain.Email;

/// <summary>
/// Thông điệp Email cần gửi qua Nhà cung cấp Email.
/// </summary>
/// <param name="To">Địa chỉ người nhận.</param>
/// <param name="Subject">Tiêu đề Email.</param>
/// <param name="HtmlBody">Nội dung HTML.</param>
/// <param name="PlainTextBody">Nội dung văn bản thuần.</param>
public sealed record EmailMessage(
    string To,
    string Subject,
    string? HtmlBody,
    string? PlainTextBody);
