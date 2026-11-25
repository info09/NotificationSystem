using System.Security.Cryptography;
using IdentityAccess.Application.Abstractions;
using IdentityAccess.Domain.IntegrationClientCredentials;

namespace IdentityAccess.Application.UserAccounts;

/// <summary>
/// Use case xoay secret Thông tin xác thực IntegrationClient.
/// </summary>
public sealed class RotateIntegrationClientCredentialUseCase
{
    private readonly IIntegrationClientCredentialRepository _credentialRepository;
    private readonly ICredentialHasher _credentialHasher;

    /// <summary>
    /// Khởi tạo use case xoay secret Thông tin xác thực IntegrationClient.
    /// </summary>
    /// <param name="credentialRepository">Repository credential IntegrationClient.</param>
    /// <param name="credentialHasher">Dịch vụ băm credential.</param>
    public RotateIntegrationClientCredentialUseCase(
        IIntegrationClientCredentialRepository credentialRepository,
        ICredentialHasher credentialHasher)
    {
        _credentialRepository = credentialRepository;
        _credentialHasher = credentialHasher;
    }

    /// <summary>
    /// Xử lý yêu cầu xoay secret credential.
    /// </summary>
    /// <param name="userAccountId">Mã định danh tài khoản.</param>
    /// <param name="credentialId">Mã định danh credential.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả xoay secret.</returns>
    public async Task<IdentityAccessAppResult<RotatedIntegrationClientCredentialResponse>> HandleAsync(
        Guid userAccountId,
        Guid credentialId,
        CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        if (userAccountId == Guid.Empty)
        {
            errors["userAccountId"] = ["UserAccountId is required."];
        }

        if (credentialId == Guid.Empty)
        {
            errors["credentialId"] = ["CredentialId is required."];
        }

        if (errors.Count > 0)
        {
            return IdentityAccessAppResult<RotatedIntegrationClientCredentialResponse>.Validation(errors);
        }

        var credential = await _credentialRepository.GetByIdAsync(credentialId, cancellationToken);
        if (credential is null || credential.UserAccountId != userAccountId)
        {
            return IdentityAccessAppResult<RotatedIntegrationClientCredentialResponse>.NotFound();
        }

        if (credential.Status == IntegrationClientCredentialStatus.Revoked)
        {
            return IdentityAccessAppResult<RotatedIntegrationClientCredentialResponse>.Validation(new Dictionary<string, string[]>
            {
                ["credentialId"] = ["Cannot rotate a revoked credential."]
            });
        }

        var plainSecret = GenerateSecret();
        credential.RotateSecret(_credentialHasher.Hash(plainSecret));
        await _credentialRepository.SaveChangesAsync(cancellationToken);

        return IdentityAccessAppResult<RotatedIntegrationClientCredentialResponse>.Ok(
            new RotatedIntegrationClientCredentialResponse(UserAccountMapping.ToDto(credential), plainSecret));
    }

    /// <summary>
    /// Sinh client secret ngẫu nhiên.
    /// </summary>
    /// <returns>Chuỗi secret plaintext.</returns>
    private static string GenerateSecret()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes).Replace("+", "-", StringComparison.Ordinal).Replace("/", "_", StringComparison.Ordinal).TrimEnd('=');
    }
}
