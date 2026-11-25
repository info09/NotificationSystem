using IdentityAccess.Application.Abstractions;
using IdentityAccess.Domain.IntegrationClientCredentials;
using IdentityAccess.Domain.UserAccounts;

namespace IdentityAccess.Application.UserAccounts;

/// <summary>
/// Use case cấp JWT cho IntegrationClient bằng client credentials.
/// </summary>
public sealed class IssueClientCredentialsTokenUseCase
{
    private readonly IIntegrationClientCredentialRepository _credentialRepository;
    private readonly IUserAccountRepository _userAccountRepository;
    private readonly ICredentialHasher _credentialHasher;
    private readonly IJwtTokenIssuer _jwtTokenIssuer;

    /// <summary>
    /// Khởi tạo use case cấp JWT cho IntegrationClient.
    /// </summary>
    /// <param name="credentialRepository">Repository credential IntegrationClient.</param>
    /// <param name="userAccountRepository">Repository tài khoản người dùng.</param>
    /// <param name="credentialHasher">Dịch vụ băm credential.</param>
    /// <param name="jwtTokenIssuer">Dịch vụ phát hành JWT.</param>
    public IssueClientCredentialsTokenUseCase(
        IIntegrationClientCredentialRepository credentialRepository,
        IUserAccountRepository userAccountRepository,
        ICredentialHasher credentialHasher,
        IJwtTokenIssuer jwtTokenIssuer)
    {
        _credentialRepository = credentialRepository;
        _userAccountRepository = userAccountRepository;
        _credentialHasher = credentialHasher;
        _jwtTokenIssuer = jwtTokenIssuer;
    }

    /// <summary>
    /// Xử lý yêu cầu cấp JWT bằng client credentials.
    /// </summary>
    /// <param name="request">Yêu cầu cấp token.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả cấp token.</returns>
    public async Task<IdentityAccessAppResult<IssueClientCredentialsTokenResponse>> HandleAsync(
        IssueClientCredentialsTokenAppRequest? request,
        CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        if (request is null)
        {
            errors["request"] = ["Request body is required."];
            return IdentityAccessAppResult<IssueClientCredentialsTokenResponse>.Validation(errors);
        }

        var clientId = request.ClientId?.Trim();
        var clientSecret = request.ClientSecret;

        if (string.IsNullOrWhiteSpace(clientId))
        {
            errors["clientId"] = ["ClientId is required."];
        }

        if (string.IsNullOrWhiteSpace(clientSecret))
        {
            errors["clientSecret"] = ["ClientSecret is required."];
        }

        if (errors.Count > 0)
        {
            return IdentityAccessAppResult<IssueClientCredentialsTokenResponse>.Validation(errors);
        }

        var credential = await _credentialRepository.GetByClientIdAsync(clientId!, cancellationToken);
        if (credential is null ||
            credential.Status != IntegrationClientCredentialStatus.Active ||
            (credential.ExpiresAtUtc.HasValue && credential.ExpiresAtUtc.Value <= DateTime.UtcNow) ||
            !_credentialHasher.Verify(credential.ClientSecretHash, clientSecret!))
        {
            return IdentityAccessAppResult<IssueClientCredentialsTokenResponse>.Unauthorized();
        }

        var account = await _userAccountRepository.GetByIdAsync(credential.UserAccountId, cancellationToken);
        if (account is null ||
            account.Status != UserAccountStatus.Active ||
            account.Role != UserRole.IntegrationClient)
        {
            return IdentityAccessAppResult<IssueClientCredentialsTokenResponse>.Unauthorized();
        }

        var token = _jwtTokenIssuer.IssueAccessToken(
            account.Id.ToString(),
            account.Role.ToString(),
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["client_id"] = credential.ClientId
            });

        return IdentityAccessAppResult<IssueClientCredentialsTokenResponse>.Ok(
            new IssueClientCredentialsTokenResponse(token.AccessToken, token.TokenType, token.ExpiresInSeconds));
    }
}
