namespace Delivery.Infrastructure.Email;

/// <summary>
/// Cấu hình SMTP cho Nhà cung cấp Email.
/// </summary>
public sealed class SmtpOptions
{
    /// <summary>
    /// Khóa cấu hình SMTP.
    /// </summary>
    public const string SectionName = "Smtp";

    /// <summary>
    /// Host SMTP.
    /// </summary>
    public string Host { get; set; } = "mailpit";

    /// <summary>
    /// Port SMTP.
    /// </summary>
    public int Port { get; set; } = 1025;

    /// <summary>
    /// Tên đăng nhập SMTP.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Mật khẩu SMTP.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Địa chỉ gửi mặc định.
    /// </summary>
    public string From { get; set; } = "no-reply@notification.local";

    /// <summary>
    /// Bật SSL cho kết nối SMTP.
    /// </summary>
    public bool EnableSsl { get; set; }
}
