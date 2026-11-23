using Delivery.Application.Abstractions;
using Delivery.Application.Email;
using Delivery.Domain.Email;

namespace UnitTests.Delivery;

/// <summary>
/// Unit test cho dịch vụ gửi Email.
/// </summary>
public sealed class EmailSenderTests
{
    [Fact]
    public async Task SendAsync_WithPlainTextOnly_GeneratesHtmlFallback()
    {
        var provider = new FakeEmailProvider();
        var sender = new EmailSender(provider, new FakeTemplateRenderer());

        await sender.SendAsync(new SendEmailRequest("user@example.com", "Subject", "Hello", null, null, null), CancellationToken.None);

        Assert.NotNull(provider.LastMessage);
        Assert.Equal("Hello", provider.LastMessage!.PlainTextBody);
        Assert.Contains("Hello", provider.LastMessage.HtmlBody);
    }

    [Fact]
    public async Task SendAsync_WithHtmlOnly_GeneratesPlainTextFallback()
    {
        var provider = new FakeEmailProvider();
        var sender = new EmailSender(provider, new FakeTemplateRenderer());

        await sender.SendAsync(new SendEmailRequest("user@example.com", "Subject", null, "<p>Hello<br/>World</p>", null, null), CancellationToken.None);

        Assert.NotNull(provider.LastMessage);
        Assert.Contains("Hello", provider.LastMessage!.PlainTextBody);
        Assert.Contains("World", provider.LastMessage.PlainTextBody);
    }

    [Fact]
    public async Task SendAsync_WithTemplateId_WhenRendererNotSupported_Throws()
    {
        var sender = new EmailSender(new FakeEmailProvider(), new ThrowingTemplateRenderer());

        await Assert.ThrowsAsync<NotSupportedException>(() =>
            sender.SendAsync(new SendEmailRequest("user@example.com", "Subject", null, null, Guid.NewGuid(), null), CancellationToken.None));
    }

    private sealed class FakeEmailProvider : IEmailProvider
    {
        public EmailMessage? LastMessage { get; private set; }

        public Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
        {
            LastMessage = message;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeTemplateRenderer : IEmailTemplateRenderer
    {
        public Task<EmailTemplateRenderResult> RenderAsync(string? subject, string? content, string? htmlContent, Guid? templateId, IReadOnlyDictionary<string, string>? templateData, CancellationToken cancellationToken)
            => Task.FromResult(new EmailTemplateRenderResult(subject ?? string.Empty, htmlContent, content));
    }

    private sealed class ThrowingTemplateRenderer : IEmailTemplateRenderer
    {
        public Task<EmailTemplateRenderResult> RenderAsync(string? subject, string? content, string? htmlContent, Guid? templateId, IReadOnlyDictionary<string, string>? templateData, CancellationToken cancellationToken)
            => throw new NotSupportedException("Email template rendering is not configured.");
    }
}
