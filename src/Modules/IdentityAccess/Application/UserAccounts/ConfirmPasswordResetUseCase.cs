using IdentityAccess.Application.Abstractions;
using IdentityAccess.Domain.UserAccounts;

namespace IdentityAccess.Application.UserAccounts;

/// <summary>
/// Use case xác nhận đặt lại mật khẩu.
/// </summary>
public sealed class ConfirmPasswordResetUseCase
{
    private readonly IUserAccountRepository _userAccountRepository;
    private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;
    private readonly ICredentialHasher _credentialHasher;

    /// <summary>
    /// Khởi tạo use case xác nhận đặt lại mật khẩu.
    /// </summary>
    /// <param name="userAccountRepository">Repository tài khoản người dùng.</param>
    /// <param name="passwordResetTokenRepository">Repository mã đặt lại mật khẩu.</param>
    /// <param name="credentialHasher">Dịch vụ băm credential.</param>
    public ConfirmPasswordResetUseCase(
        IUserAccountRepository userAccountRepository,
        IPasswordResetTokenRepository passwordResetTokenRepository,
        ICredentialHasher credentialHasher)
    {
        _userAccountRepository = userAccountRepository;
        _passwordResetTokenRepository = passwordResetTokenRepository;
        _credentialHasher = credentialHasher;
    }

    /// <summary>
    /// Xử lý yêu cầu xác nhận đặt lại mật khẩu.
    /// </summary>
    /// <param name="request">Yêu cầu xác nhận đặt lại mật khẩu.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả đặt lại mật khẩu.</returns>
    public async Task<IdentityAccessAppResult<ConfirmPasswordResetResponse>> HandleAsync(
        ConfirmPasswordResetAppRequest? request,
        CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        if (request is null)
        {
            errors["request"] = ["Request body is required."];
            return IdentityAccessAppResult<ConfirmPasswordResetResponse>.Validation(errors);
        }

        if (string.IsNullOrWhiteSpace(request.Token))
        {
            errors["token"] = ["Token is required."];
        }

        if (string.IsNullOrWhiteSpace(request.NewPassword))
        {
            errors["newPassword"] = ["New password is required."];
        }
        else if (request.NewPassword.Length < 8)
        {
            errors["newPassword"] = ["New password must be at least 8 characters."];
        }

        if (errors.Count > 0)
        {
            return IdentityAccessAppResult<ConfirmPasswordResetResponse>.Validation(errors);
        }

        var utcNow = DateTime.UtcNow;
        var tokenHash = PasswordResetTokenHashing.Hash(request.Token!.Trim());
        var resetToken = await _passwordResetTokenRepository.GetByTokenHashAsync(tokenHash, cancellationToken);

        if (resetToken is null || resetToken.IsUsed() || resetToken.IsExpired(utcNow))
        {
            return IdentityAccessAppResult<ConfirmPasswordResetResponse>.Validation(new Dictionary<string, string[]>
            {
                ["token"] = ["Token is invalid or expired."]
            });
        }

        var account = await _userAccountRepository.GetByIdAsync(resetToken.UserAccountId, cancellationToken);
        if (account is null ||
            account.Status != UserAccountStatus.Active ||
            (account.Role != UserRole.SystemAdministrator && account.Role != UserRole.BusinessOperator))
        {
            return IdentityAccessAppResult<ConfirmPasswordResetResponse>.Validation(new Dictionary<string, string[]>
            {
                ["token"] = ["Token is invalid or expired."]
            });
        }

        account.SetPasswordHash(_credentialHasher.Hash(request.NewPassword!), utcNow);
        resetToken.MarkUsed(utcNow);

        var activeTokens = await _passwordResetTokenRepository.GetActiveByUserAccountIdAsync(account.Id, utcNow, cancellationToken);
        foreach (var activeToken in activeTokens)
        {
            activeToken.MarkUsed(utcNow);
        }

        await _userAccountRepository.SaveChangesAsync(cancellationToken);
        await _passwordResetTokenRepository.SaveChangesAsync(cancellationToken);

        return IdentityAccessAppResult<ConfirmPasswordResetResponse>.Ok(new ConfirmPasswordResetResponse(true));
    }
}
