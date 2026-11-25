using System.Security.Cryptography;
using System.Text;

namespace IdentityAccess.Application.UserAccounts;

/// <summary>
/// Cung cấp hàm băm xác định cho Mã đặt lại mật khẩu.
/// </summary>
internal static class PasswordResetTokenHashing
{
    /// <summary>
    /// Tạo giá trị băm SHA256 cho token.
    /// </summary>
    /// <param name="token">Token plaintext.</param>
    /// <returns>Chuỗi băm hex.</returns>
    public static string Hash(string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }
}
