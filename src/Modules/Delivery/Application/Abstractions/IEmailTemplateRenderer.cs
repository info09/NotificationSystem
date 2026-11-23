namespace Delivery.Application.Abstractions;

/// <summary>
/// Kết quả render mẫu Email.
/// </summary>
/// <param name="Subject">Tiêu đề Email.</param>
/// <param name="HtmlBody">Nội dung HTML.</param>
/// <param name="PlainTextBody">Nội dung văn bản thuần.</param>
public sealed record EmailTemplateRenderResult(string Subject, string? HtmlBody, string? PlainTextBody);

/// <summary>
/// Abstraction render Mẫu thông báo Email.
/// </summary>
public interface IEmailTemplateRenderer
{
    /// <summary>
    /// Render nội dung Email từ mẫu hoặc dữ liệu đầu vào.
    /// </summary>
    /// <param name="subject">Tiêu đề gốc.</param>
    /// <param name="content">Nội dung thuần gốc.</param>
    /// <param name="htmlContent">Nội dung HTML gốc.</param>
    /// <param name="templateId">Mã Mẫu thông báo.</param>
    /// <param name="templateData">Dữ liệu render mẫu.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả render Email.</returns>
    Task<EmailTemplateRenderResult> RenderAsync(
        string? subject,
        string? content,
        string? htmlContent,
        Guid? templateId,
        IReadOnlyDictionary<string, string>? templateData,
        CancellationToken cancellationToken);
}
