using Delivery.Application.Abstractions;
using Delivery.Domain.Email;
using System.Net;
using System.Text.RegularExpressions;

namespace Delivery.Application.Email;

/// <summary>
/// Dịch vụ gửi Email, áp dụng render nội dung và fallback HTML/plain-text.
/// </summary>
public sealed class EmailSender : IEmailSender
{
    private readonly IEmailProvider _emailProvider;
    private readonly IEmailTemplateRenderer _templateRenderer;

    /// <summary>
    /// Khởi tạo dịch vụ gửi Email.
    /// </summary>
    /// <param name="emailProvider">Nhà cung cấp Email.</param>
    /// <param name="templateRenderer">Bộ render mẫu Email.</param>
    public EmailSender(IEmailProvider emailProvider, IEmailTemplateRenderer templateRenderer)
    {
        _emailProvider = emailProvider;
        _templateRenderer = templateRenderer;
    }

    /// <summary>
    /// Gửi Email theo yêu cầu.
    /// </summary>
    /// <param name="request">Yêu cầu gửi Email.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    public async Task SendAsync(SendEmailRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.To))
        {
            throw new ArgumentException("Recipient email is required.", nameof(request));
        }

        var rendered = await _templateRenderer.RenderAsync(
            request.Subject,
            request.Content,
            request.HtmlContent,
            request.TemplateId,
            request.TemplateData,
            cancellationToken);

        var subject = string.IsNullOrWhiteSpace(rendered.Subject) ? "(No subject)" : rendered.Subject.Trim();
        var htmlBody = string.IsNullOrWhiteSpace(rendered.HtmlBody) ? null : rendered.HtmlBody;
        var plainTextBody = string.IsNullOrWhiteSpace(rendered.PlainTextBody) ? null : rendered.PlainTextBody;

        if (htmlBody is null && plainTextBody is null)
        {
            throw new ArgumentException("Email content is required.", nameof(request));
        }

        if (htmlBody is null && plainTextBody is not null)
        {
            htmlBody = WrapPlainTextAsHtml(plainTextBody);
        }

        if (plainTextBody is null && htmlBody is not null)
        {
            plainTextBody = ConvertHtmlToPlainText(htmlBody);
        }

        var message = new EmailMessage(request.To.Trim(), subject, htmlBody, plainTextBody);
        await _emailProvider.SendAsync(message, cancellationToken);
    }

    /// <summary>
    /// Bọc nội dung văn bản thuần thành HTML đơn giản.
    /// </summary>
    /// <param name="plainText">Nội dung văn bản thuần.</param>
    /// <returns>Nội dung HTML.</returns>
    private static string WrapPlainTextAsHtml(string plainText)
    {
        var encoded = WebUtility.HtmlEncode(plainText).Replace("\n", "<br/>");
        return $"<html><body><p>{encoded}</p></body></html>";
    }

    /// <summary>
    /// Chuyển HTML thành văn bản thuần cơ bản.
    /// </summary>
    /// <param name="html">Nội dung HTML.</param>
    /// <returns>Nội dung văn bản thuần.</returns>
    private static string ConvertHtmlToPlainText(string html)
    {
        var withLineBreaks = Regex.Replace(html, "<\\s*br\\s*/?>", "\n", RegexOptions.IgnoreCase);
        withLineBreaks = Regex.Replace(withLineBreaks, "</\\s*p\\s*>", "\n", RegexOptions.IgnoreCase);
        var stripped = Regex.Replace(withLineBreaks, "<.*?>", string.Empty, RegexOptions.Singleline);
        return WebUtility.HtmlDecode(stripped).Trim();
    }
}
