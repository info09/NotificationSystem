namespace IdentityAccess.Application.Abstractions;

/// <summary>
/// Dịch vụ tạo liên kết đặt lại mật khẩu.
/// </summary>
public interface IPasswordResetLinkFactory
{
    /// <summary>
    /// Tạo liên kết đặt lại mật khẩu từ token plaintext.
    /// </summary>
    /// <param name="rawToken">Token plaintext.</param>
    /// <returns>Liên kết đặt lại mật khẩu.</returns>
    string CreateResetLink(string rawToken);

    /// <summary>
    /// Lấy thời hạn hiệu lực của token đặt lại mật khẩu.
    /// </summary>
    /// <returns>Thời hạn hiệu lực.</returns>
    TimeSpan GetTokenLifetime();
}
