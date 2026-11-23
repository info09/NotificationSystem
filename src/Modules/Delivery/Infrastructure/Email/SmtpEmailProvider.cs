using Delivery.Application.Abstractions;
using Delivery.Domain.Email;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace Delivery.Infrastructure.Email;

/// <summary>
/// Nhà cung cấp Email gửi Thông điệp qua SMTP.
/// </summary>
public sealed class SmtpEmailProvider : IEmailProvider
{
    private readonly SmtpOptions _options;

    /// <summary>
    /// Khởi tạo Nhà cung cấp Email SMTP.
    /// </summary>
    /// <param name="options">Cấu hình SMTP.</param>
    public SmtpEmailProvider(IOptions<SmtpOptions> options)
    {
        _options = options.Value;
    }

    /// <summary>
    /// Gửi Thông điệp Email qua SMTP.
    /// </summary>
    /// <param name="message">Thông điệp Email cần gửi.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (string.IsNullOrWhiteSpace(message.To))
        {
            throw new ArgumentException("Recipient email is required.", nameof(message));
        }

        using var mailMessage = new MailMessage
        {
            From = new MailAddress(_options.From),
            Subject = message.Subject ?? string.Empty,
            Body = message.HtmlBody ?? message.PlainTextBody ?? string.Empty,
            IsBodyHtml = !string.IsNullOrWhiteSpace(message.HtmlBody),
            BodyEncoding = Encoding.UTF8,
            SubjectEncoding = Encoding.UTF8,
            HeadersEncoding = Encoding.UTF8
        };
        mailMessage.To.Add(message.To);

        if (!string.IsNullOrWhiteSpace(message.PlainTextBody) && !string.IsNullOrWhiteSpace(message.HtmlBody))
        {
            mailMessage.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(message.PlainTextBody, Encoding.UTF8, "text/plain"));
            mailMessage.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(message.HtmlBody, Encoding.UTF8, "text/html"));
        }

        using var client = new SmtpClient(_options.Host, _options.Port)
        {
            EnableSsl = _options.EnableSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network
        };

        if (!string.IsNullOrWhiteSpace(_options.Username))
        {
            client.Credentials = new NetworkCredential(_options.Username, _options.Password);
        }

        cancellationToken.ThrowIfCancellationRequested();
        await client.SendMailAsync(mailMessage, cancellationToken);
    }
}
