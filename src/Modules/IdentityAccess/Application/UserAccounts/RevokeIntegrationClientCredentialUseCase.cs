using IdentityAccess.Application.Abstractions;
using IdentityAccess.Domain.IntegrationClientCredentials;

namespace IdentityAccess.Application.UserAccounts;

/// <summary>
/// Use case vô hiệu hóa Thông tin xác thực IntegrationClient.
/// </summary>
public sealed class RevokeIntegrationClientCredentialUseCase
{
    private readonly IIntegrationClientCredentialRepository _credentialRepository;

    /// <summary>
    /// Khởi tạo use case vô hiệu hóa Thông tin xác thực IntegrationClient.
    /// </summary>
    /// <param name="credentialRepository">Repository credential IntegrationClient.</param>
    public RevokeIntegrationClientCredentialUseCase(IIntegrationClientCredentialRepository credentialRepository)
    {
        _credentialRepository = credentialRepository;
    }

    /// <summary>
    /// Xử lý yêu cầu vô hiệu hóa credential.
    /// </summary>
    /// <param name="userAccountId">Mã định danh tài khoản.</param>
    /// <param name="credentialId">Mã định danh credential.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả vô hiệu hóa credential.</returns>
    public async Task<IdentityAccessAppResult<IntegrationClientCredentialDto>> HandleAsync(
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
            return IdentityAccessAppResult<IntegrationClientCredentialDto>.Validation(errors);
        }

        var credential = await _credentialRepository.GetByIdAsync(credentialId, cancellationToken);
        if (credential is null || credential.UserAccountId != userAccountId)
        {
            return IdentityAccessAppResult<IntegrationClientCredentialDto>.NotFound();
        }

        credential.Revoke();
        await _credentialRepository.SaveChangesAsync(cancellationToken);
        return IdentityAccessAppResult<IntegrationClientCredentialDto>.Ok(UserAccountMapping.ToDto(credential));
    }
}
