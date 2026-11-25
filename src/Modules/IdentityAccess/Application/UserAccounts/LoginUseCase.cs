using IdentityAccess.Application.Abstractions;
using IdentityAccess.Domain.UserAccounts;

namespace IdentityAccess.Application.UserAccounts;

/// <summary>
/// Use case đăng nhập bằng credential nội bộ cho AdminUI.
/// </summary>
public sealed class LoginUseCase
{
    private readonly IUserAccountRepository _userAccountRepository;
    private readonly ICredentialHasher _credentialHasher;
    private readonly IJwtTokenIssuer _jwtTokenIssuer;

    /// <summary>
    /// Khởi tạo use case đăng nhập.
    /// </summary>
    /// <param name="userAccountRepository">Repository tài khoản người dùng.</param>
    /// <param name="credentialHasher">Dịch vụ băm credential.</param>
    /// <param name="jwtTokenIssuer">Dịch vụ phát hành JWT.</param>
    public LoginUseCase(
        IUserAccountRepository userAccountRepository,
        ICredentialHasher credentialHasher,
        IJwtTokenIssuer jwtTokenIssuer)
    {
        _userAccountRepository = userAccountRepository;
        _credentialHasher = credentialHasher;
        _jwtTokenIssuer = jwtTokenIssuer;
    }

    /// <summary>
    /// Xử lý yêu cầu đăng nhập bằng credential nội bộ.
    /// </summary>
    /// <param name="request">Yêu cầu đăng nhập.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả đăng nhập.</returns>
    public async Task<IdentityAccessAppResult<LoginResponse>> HandleAsync(
        LoginAppRequest? request,
        CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        if (request is null)
        {
            errors["request"] = ["Request body is required."];
            return IdentityAccessAppResult<LoginResponse>.Validation(errors);
        }

        var userName = request.UserName?.Trim();
        if (string.IsNullOrWhiteSpace(userName))
        {
            errors["userName"] = ["UserName is required."];
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            errors["password"] = ["Password is required."];
        }

        if (errors.Count > 0)
        {
            return IdentityAccessAppResult<LoginResponse>.Validation(errors);
        }

        var account = await _userAccountRepository.GetByUserNameAsync(userName!, cancellationToken);
        if (account is null ||
            account.Status != UserAccountStatus.Active ||
            (account.Role != UserRole.SystemAdministrator && account.Role != UserRole.BusinessOperator) ||
            string.IsNullOrWhiteSpace(account.PasswordHash) ||
            !_credentialHasher.Verify(account.PasswordHash, request.Password!))
        {
            return IdentityAccessAppResult<LoginResponse>.Unauthorized();
        }

        var token = _jwtTokenIssuer.IssueAccessToken(account.Id.ToString(), account.Role.ToString());
        return IdentityAccessAppResult<LoginResponse>.Ok(
            new LoginResponse(token.AccessToken, token.TokenType, token.ExpiresInSeconds));
    }
}
