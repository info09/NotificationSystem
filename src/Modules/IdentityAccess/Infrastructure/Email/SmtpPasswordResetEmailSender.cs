using System.Net;
using System.Net.Mail;
using System.Text;
using IdentityAccess.Application.Abstractions;
using Microsoft.Extensions.Options;

namespace IdentityAccess.Infrastructure.Email;

/// <summary>
/// Dịch vụ gửi email đặt lại mật khẩu qua SMTP.
/// </summary>
public sealed class SmtpPasswordResetEmailSender : IPasswordResetEmailSender
{
    private readonly SmtpOptions _options;

    /// <summary>
    /// Khởi tạo dịch vụ gửi email đặt lại mật khẩu.
    /// </summary>
    /// <param name="options">Cấu hình SMTP.</param>
    public SmtpPasswordResetEmailSender(IOptions<SmtpOptions> options)
    {
        _options = options.Value;
    }

    /// <inheritdoc />
    public async Task SendPasswordResetEmailAsync(string toEmail, string displayName, string resetLink, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(toEmail);
        ArgumentException.ThrowIfNullOrWhiteSpace(resetLink);

        var safeName = WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(displayName) ? toEmail : displayName.Trim());
        var safeLink = WebUtility.HtmlEncode(resetLink);
        var subject = "Đặt lại mật khẩu - Notification System";
        var plainText = $"Xin chào {safeName},\n\nBạn đã yêu cầu đặt lại mật khẩu. Mở liên kết sau để đặt lại mật khẩu:\n{resetLink}\n\nNếu bạn không yêu cầu, hãy bỏ qua email này.";
        var html = $"<p>Xin chào {safeName},</p><p>Bạn đã yêu cầu đặt lại mật khẩu. Nhấn liên kết sau để đặt lại mật khẩu:</p><p><a href=\"{safeLink}\">{safeLink}</a></p><p>Nếu bạn không yêu cầu, hãy bỏ qua email này.</p>";

        using var mailMessage = new MailMessage
        {
            From = new MailAddress(_options.From),
            Subject = subject,
            Body = html,
            IsBodyHtml = true,
            BodyEncoding = Encoding.UTF8,
            SubjectEncoding = Encoding.UTF8,
            HeadersEncoding = Encoding.UTF8
        };
        mailMessage.To.Add(toEmail);
        mailMessage.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(plainText, Encoding.UTF8, "text/plain"));
        mailMessage.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(html, Encoding.UTF8, "text/html"));

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
