using Delivery.Application.Abstractions;

namespace Delivery.Infrastructure.Email;

/// <summary>
/// Bộ render mẫu Email mặc định, dùng nội dung có sẵn và chưa hỗ trợ render template thật.
/// </summary>
public sealed class PassThroughEmailTemplateRenderer : IEmailTemplateRenderer
{
    /// <summary>
    /// Render nội dung Email từ dữ liệu đầu vào.
    /// </summary>
    /// <param name="subject">Tiêu đề gốc.</param>
    /// <param name="content">Nội dung thuần gốc.</param>
    /// <param name="htmlContent">Nội dung HTML gốc.</param>
    /// <param name="templateId">Mã Mẫu thông báo.</param>
    /// <param name="templateData">Dữ liệu render mẫu.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả render Email.</returns>
    public Task<EmailTemplateRenderResult> RenderAsync(
        string? subject,
        string? content,
        string? htmlContent,
        Guid? templateId,
        IReadOnlyDictionary<string, string>? templateData,
        CancellationToken cancellationToken)
    {
        if (templateId.HasValue)
        {
            throw new NotSupportedException("Email template rendering is not configured.");
        }

        return Task.FromResult(new EmailTemplateRenderResult(subject ?? string.Empty, htmlContent, content));
    }
}
