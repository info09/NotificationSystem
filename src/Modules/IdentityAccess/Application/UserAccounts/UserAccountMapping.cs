using IdentityAccess.Domain.IntegrationClientCredentials;
using IdentityAccess.Domain.UserAccounts;

namespace IdentityAccess.Application.UserAccounts;

/// <summary>
/// Cung cấp hàm ánh xạ cho Tài khoản người dùng và credential.
/// </summary>
internal static class UserAccountMapping
{
    /// <summary>
    /// Ánh xạ thực thể Tài khoản người dùng sang DTO.
    /// </summary>
    /// <param name="account">Thực thể tài khoản.</param>
    /// <returns>DTO tài khoản.</returns>
    public static UserAccountDto ToDto(UserAccount account) =>
        new(
            account.Id,
            account.UserName,
            account.Email,
            account.DisplayName,
            account.Role.ToString(),
            account.Status.ToString(),
            account.CreatedAtUtc,
            account.UpdatedAtUtc);

    /// <summary>
    /// Ánh xạ thực thể Thông tin xác thực IntegrationClient sang DTO.
    /// </summary>
    /// <param name="credential">Thực thể credential.</param>
    /// <returns>DTO credential.</returns>
    public static IntegrationClientCredentialDto ToDto(IntegrationClientCredential credential) =>
        new(
            credential.Id,
            credential.UserAccountId,
            credential.ClientId,
            credential.Status.ToString(),
            credential.CreatedAtUtc,
            credential.ExpiresAtUtc,
            credential.RevokedAtUtc,
            credential.LastRotatedAtUtc);
}
