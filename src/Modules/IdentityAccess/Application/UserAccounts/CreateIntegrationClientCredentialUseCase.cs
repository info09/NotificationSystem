using System.Security.Cryptography;
using IdentityAccess.Application.Abstractions;
using IdentityAccess.Domain.IntegrationClientCredentials;
using IdentityAccess.Domain.UserAccounts;

namespace IdentityAccess.Application.UserAccounts;

/// <summary>
/// Use case tạo Thông tin xác thực IntegrationClient.
/// </summary>
public sealed class CreateIntegrationClientCredentialUseCase
{
    private readonly IUserAccountRepository _userAccountRepository;
    private readonly IIntegrationClientCredentialRepository _credentialRepository;
    private readonly ICredentialHasher _credentialHasher;

    /// <summary>
    /// Khởi tạo use case tạo Thông tin xác thực IntegrationClient.
    /// </summary>
    /// <param name="userAccountRepository">Repository tài khoản người dùng.</param>
    /// <param name="credentialRepository">Repository credential IntegrationClient.</param>
    /// <param name="credentialHasher">Dịch vụ băm credential.</param>
    public CreateIntegrationClientCredentialUseCase(
        IUserAccountRepository userAccountRepository,
        IIntegrationClientCredentialRepository credentialRepository,
        ICredentialHasher credentialHasher)
    {
        _userAccountRepository = userAccountRepository;
        _credentialRepository = credentialRepository;
        _credentialHasher = credentialHasher;
    }

    /// <summary>
    /// Xử lý yêu cầu tạo Thông tin xác thực IntegrationClient.
    /// </summary>
    /// <param name="userAccountId">Mã định danh tài khoản.</param>
    /// <param name="request">Yêu cầu tạo credential.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả tạo credential.</returns>
    public async Task<IdentityAccessAppResult<CreatedIntegrationClientCredentialResponse>> HandleAsync(
        Guid userAccountId,
        CreateIntegrationClientCredentialAppRequest? request,
        CancellationToken cancellationToken)
    {
        if (userAccountId == Guid.Empty)
        {
            return IdentityAccessAppResult<CreatedIntegrationClientCredentialResponse>.Validation(new Dictionary<string, string[]>
            {
                ["userAccountId"] = ["UserAccountId is required."]
            });
        }

        request ??= new CreateIntegrationClientCredentialAppRequest(null, null);

        var account = await _userAccountRepository.GetByIdAsync(userAccountId, cancellationToken);
        if (account is null || account.Status == UserAccountStatus.Deleted)
        {
            return IdentityAccessAppResult<CreatedIntegrationClientCredentialResponse>.NotFound();
        }

        if (account.Role != UserRole.IntegrationClient)
        {
            return IdentityAccessAppResult<CreatedIntegrationClientCredentialResponse>.Validation(new Dictionary<string, string[]>
            {
                ["role"] = ["Account must have IntegrationClient role."]
            });
        }

        var clientId = string.IsNullOrWhiteSpace(request.ClientId)
            ? $"ic_{Guid.NewGuid():N}"
            : request.ClientId.Trim();

        if (await _credentialRepository.ExistsByClientIdAsync(clientId, null, cancellationToken))
        {
            return IdentityAccessAppResult<CreatedIntegrationClientCredentialResponse>.Conflict();
        }

        var plainSecret = GenerateSecret();
        var credential = IntegrationClientCredential.Create(
            Guid.NewGuid(),
            userAccountId,
            clientId,
            _credentialHasher.Hash(plainSecret),
            request.ExpiresAtUtc);

        await _credentialRepository.AddAsync(credential, cancellationToken);
        await _credentialRepository.SaveChangesAsync(cancellationToken);

        var response = new CreatedIntegrationClientCredentialResponse(
            UserAccountMapping.ToDto(credential),
            plainSecret);
        return IdentityAccessAppResult<CreatedIntegrationClientCredentialResponse>.Created(response);
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
