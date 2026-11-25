using IdentityAccess.Application.Abstractions;
using IdentityAccess.Domain.UserAccounts;

namespace IdentityAccess.Application.UserAccounts;

/// <summary>
/// Use case lấy danh sách Thông tin xác thực IntegrationClient.
/// </summary>
public sealed class GetIntegrationClientCredentialsUseCase
{
    private readonly IUserAccountRepository _userAccountRepository;
    private readonly IIntegrationClientCredentialRepository _credentialRepository;

    /// <summary>
    /// Khởi tạo use case lấy danh sách Thông tin xác thực IntegrationClient.
    /// </summary>
    /// <param name="userAccountRepository">Repository tài khoản người dùng.</param>
    /// <param name="credentialRepository">Repository credential IntegrationClient.</param>
    public GetIntegrationClientCredentialsUseCase(
        IUserAccountRepository userAccountRepository,
        IIntegrationClientCredentialRepository credentialRepository)
    {
        _userAccountRepository = userAccountRepository;
        _credentialRepository = credentialRepository;
    }

    /// <summary>
    /// Xử lý yêu cầu lấy danh sách credential theo Tài khoản người dùng.
    /// </summary>
    /// <param name="userAccountId">Mã định danh tài khoản.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả danh sách credential.</returns>
    public async Task<IdentityAccessAppResult<IReadOnlyList<IntegrationClientCredentialDto>>> HandleAsync(
        Guid userAccountId,
        CancellationToken cancellationToken)
    {
        if (userAccountId == Guid.Empty)
        {
            return IdentityAccessAppResult<IReadOnlyList<IntegrationClientCredentialDto>>.Validation(new Dictionary<string, string[]>
            {
                ["userAccountId"] = ["UserAccountId is required."]
            });
        }

        var account = await _userAccountRepository.GetByIdAsync(userAccountId, cancellationToken);
        if (account is null || account.Status == UserAccountStatus.Deleted)
        {
            return IdentityAccessAppResult<IReadOnlyList<IntegrationClientCredentialDto>>.NotFound();
        }

        var credentials = await _credentialRepository.GetByUserAccountIdAsync(userAccountId, cancellationToken);
        var dtos = credentials.Select(UserAccountMapping.ToDto).ToList();
        return IdentityAccessAppResult<IReadOnlyList<IntegrationClientCredentialDto>>.Ok(dtos);
    }
}
