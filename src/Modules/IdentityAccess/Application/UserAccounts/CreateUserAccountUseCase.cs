using IdentityAccess.Application.Abstractions;
using IdentityAccess.Domain.UserAccounts;

namespace IdentityAccess.Application.UserAccounts;

/// <summary>
/// Use case tạo Tài khoản người dùng.
/// </summary>
public sealed class CreateUserAccountUseCase
{
    private readonly IUserAccountRepository _userAccountRepository;
    private readonly ICredentialHasher _credentialHasher;

    /// <summary>
    /// Khởi tạo use case tạo Tài khoản người dùng.
    /// </summary>
    /// <param name="userAccountRepository">Repository tài khoản người dùng.</param>
    /// <param name="credentialHasher">Dịch vụ băm credential.</param>
    public CreateUserAccountUseCase(
        IUserAccountRepository userAccountRepository,
        ICredentialHasher credentialHasher)
    {
        _userAccountRepository = userAccountRepository;
        _credentialHasher = credentialHasher;
    }

    /// <summary>
    /// Xử lý yêu cầu tạo Tài khoản người dùng.
    /// </summary>
    /// <param name="request">Yêu cầu tạo tài khoản.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả tạo tài khoản.</returns>
    public async Task<IdentityAccessAppResult<UserAccountDto>> HandleAsync(
        CreateUserAccountAppRequest? request,
        CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        if (request is null)
        {
            errors["request"] = ["Request body is required."];
            return IdentityAccessAppResult<UserAccountDto>.Validation(errors);
        }

        var userName = request.UserName?.Trim();
        var email = request.Email?.Trim();
        var displayName = request.DisplayName?.Trim();
        var roleText = request.Role?.Trim();

        if (string.IsNullOrWhiteSpace(userName))
        {
            errors["userName"] = ["UserName is required."];
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            errors["email"] = ["Email is required."];
        }

        if (string.IsNullOrWhiteSpace(displayName))
        {
            errors["displayName"] = ["DisplayName is required."];
        }

        if (!Enum.TryParse<UserRole>(roleText, true, out var role))
        {
            errors["role"] = ["Role is invalid."];
        }

        if (errors.Count == 0 && role is UserRole.SystemAdministrator or UserRole.BusinessOperator &&
            string.IsNullOrWhiteSpace(request.Password))
        {
            errors["password"] = ["Password is required for SystemAdministrator or BusinessOperator."];
        }

        if (errors.Count == 0 && !string.IsNullOrWhiteSpace(request.Password) && request.Password.Length < 8)
        {
            errors["password"] = ["Password must be at least 8 characters."];
        }

        if (errors.Count > 0)
        {
            return IdentityAccessAppResult<UserAccountDto>.Validation(errors);
        }

        if (await _userAccountRepository.ExistsByUserNameAsync(userName!, null, cancellationToken))
        {
            return IdentityAccessAppResult<UserAccountDto>.Conflict();
        }

        if (await _userAccountRepository.ExistsByEmailAsync(email!, null, cancellationToken))
        {
            return IdentityAccessAppResult<UserAccountDto>.Conflict();
        }

        var passwordHash = string.IsNullOrWhiteSpace(request.Password)
            ? null
            : _credentialHasher.Hash(request.Password);

        var account = UserAccount.Create(
            Guid.NewGuid(),
            userName!,
            email!,
            displayName!,
            role,
            passwordHash);

        await _userAccountRepository.AddAsync(account, cancellationToken);
        await _userAccountRepository.SaveChangesAsync(cancellationToken);

        return IdentityAccessAppResult<UserAccountDto>.Created(UserAccountMapping.ToDto(account));
    }
}
