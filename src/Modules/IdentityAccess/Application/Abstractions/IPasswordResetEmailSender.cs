namespace IdentityAccess.Application.Abstractions;

/// <summary>
/// Dịch vụ gửi email đặt lại mật khẩu.
/// </summary>
public interface IPasswordResetEmailSender
{
    /// <summary>
    /// Gửi email chứa liên kết đặt lại mật khẩu.
    /// </summary>
    /// <param name="toEmail">Email người nhận.</param>
    /// <param name="displayName">Tên hiển thị người nhận.</param>
    /// <param name="resetLink">Liên kết đặt lại mật khẩu.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    Task SendPasswordResetEmailAsync(string toEmail, string displayName, string resetLink, CancellationToken cancellationToken);
}
