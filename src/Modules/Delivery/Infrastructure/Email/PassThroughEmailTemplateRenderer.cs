using System.Net;
using System.Text.RegularExpressions;
using BuildingBlocks.SharedKernel.Notifications;
using Delivery.Application.Abstractions;

namespace Delivery.Infrastructure.Email;

/// <summary>
/// Bộ render mẫu Email, hỗ trợ render Mẫu thông báo và fallback nội dung.
/// </summary>
public sealed class PassThroughEmailTemplateRenderer : IEmailTemplateRenderer
{
    private static readonly Regex PlaceholderRegex = new("\\{\\{\\s*(?<key>[a-zA-Z0-9_.-]+)\\s*\\}\\}", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private readonly INotificationTemplateResolver _templateResolver;

    /// <summary>
    /// Khởi tạo bộ render mẫu Email.
    /// </summary>
    /// <param name="templateResolver">Bộ phân giải Mẫu thông báo.</param>
    public PassThroughEmailTemplateRenderer(INotificationTemplateResolver templateResolver)
    {
        _templateResolver = templateResolver;
    }

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
    public async Task<EmailTemplateRenderResult> RenderAsync(
        string? subject,
        string? content,
        string? htmlContent,
        Guid? templateId,
        IReadOnlyDictionary<string, string>? templateData,
        CancellationToken cancellationToken)
    {
        if (templateId.HasValue)
        {
            var template = await _templateResolver.ResolveAsync(templateId.Value, cancellationToken);
            if (template is null)
            {
                throw new InvalidOperationException($"Notification template '{templateId.Value}' was not found.");
            }

            if (!template.IsActive)
            {
                throw new InvalidOperationException($"Notification template '{templateId.Value}' is inactive.");
            }

            var resolvedSubject = RenderPlaceholders(subject ?? template.Subject, templateData);
            var resolvedBody = RenderPlaceholders(content ?? template.Body, templateData);
            var resolvedHtml = RenderPlaceholders(htmlContent, templateData);

            if (string.IsNullOrWhiteSpace(resolvedHtml) && LooksLikeHtml(resolvedBody))
            {
                resolvedHtml = resolvedBody;
                resolvedBody = null;
            }

            return new EmailTemplateRenderResult(resolvedSubject ?? string.Empty, resolvedHtml, resolvedBody);
        }

        return new EmailTemplateRenderResult(
            RenderPlaceholders(subject, templateData) ?? string.Empty,
            RenderPlaceholders(htmlContent, templateData),
            RenderPlaceholders(content, templateData));
    }

    private static string? RenderPlaceholders(string? value, IReadOnlyDictionary<string, string>? templateData)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        if (templateData is null || templateData.Count == 0)
        {
            return value;
        }

        return PlaceholderRegex.Replace(value, match =>
        {
            var key = match.Groups["key"].Value;
            return templateData.TryGetValue(key, out var replacement)
                ? replacement ?? string.Empty
                : match.Value;
        });
    }

    private static bool LooksLikeHtml(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return value.Contains('<') && value.Contains('>');
    }
}
