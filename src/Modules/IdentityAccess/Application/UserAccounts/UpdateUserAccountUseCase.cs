using IdentityAccess.Application.Abstractions;
using IdentityAccess.Domain.UserAccounts;

namespace IdentityAccess.Application.UserAccounts;

/// <summary>
/// Use case cập nhật Tài khoản người dùng.
/// </summary>
public sealed class UpdateUserAccountUseCase
{
    private readonly IUserAccountRepository _userAccountRepository;
    private readonly ICredentialHasher _credentialHasher;

    /// <summary>
    /// Khởi tạo use case cập nhật Tài khoản người dùng.
    /// </summary>
    /// <param name="userAccountRepository">Repository tài khoản người dùng.</param>
    /// <param name="credentialHasher">Dịch vụ băm credential.</param>
    public UpdateUserAccountUseCase(
        IUserAccountRepository userAccountRepository,
        ICredentialHasher credentialHasher)
    {
        _userAccountRepository = userAccountRepository;
        _credentialHasher = credentialHasher;
    }

    /// <summary>
    /// Xử lý yêu cầu cập nhật Tài khoản người dùng.
    /// </summary>
    /// <param name="id">Mã định danh tài khoản.</param>
    /// <param name="request">Yêu cầu cập nhật.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả cập nhật.</returns>
    public async Task<IdentityAccessAppResult<UserAccountDto>> HandleAsync(
        Guid id,
        UpdateUserAccountAppRequest? request,
        CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        if (id == Guid.Empty)
        {
            errors["id"] = ["Id is required."];
        }

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

        if (errors.Count == 0 && !string.IsNullOrWhiteSpace(request.Password) && request.Password.Length < 8)
        {
            errors["password"] = ["Password must be at least 8 characters."];
        }

        if (errors.Count > 0)
        {
            return IdentityAccessAppResult<UserAccountDto>.Validation(errors);
        }

        var account = await _userAccountRepository.GetByIdAsync(id, cancellationToken);
        if (account is null || account.Status == UserAccountStatus.Deleted)
        {
            return IdentityAccessAppResult<UserAccountDto>.NotFound();
        }

        if (await _userAccountRepository.ExistsByUserNameAsync(userName!, id, cancellationToken))
        {
            return IdentityAccessAppResult<UserAccountDto>.Conflict();
        }

        if (await _userAccountRepository.ExistsByEmailAsync(email!, id, cancellationToken))
        {
            return IdentityAccessAppResult<UserAccountDto>.Conflict();
        }

        if (role is UserRole.SystemAdministrator or UserRole.BusinessOperator &&
            string.IsNullOrWhiteSpace(request.Password) &&
            string.IsNullOrWhiteSpace(account.PasswordHash))
        {
            return IdentityAccessAppResult<UserAccountDto>.Validation(new Dictionary<string, string[]>
            {
                ["password"] = ["Password is required for SystemAdministrator or BusinessOperator."]
            });
        }

        account.UpdateProfile(userName!, email!, displayName!);
        account.AssignRole(role);

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            account.SetPasswordHash(_credentialHasher.Hash(request.Password));
        }

        await _userAccountRepository.SaveChangesAsync(cancellationToken);
        return IdentityAccessAppResult<UserAccountDto>.Ok(UserAccountMapping.ToDto(account));
    }
}
