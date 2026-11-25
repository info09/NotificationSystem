using IdentityAccess.Application.Abstractions;
using IdentityAccess.Domain.UserAccounts;

namespace IdentityAccess.Application.UserAccounts;

/// <summary>
/// Use case mở khóa Tài khoản người dùng.
/// </summary>
public sealed class UnlockUserAccountUseCase
{
    private readonly IUserAccountRepository _userAccountRepository;

    /// <summary>
    /// Khởi tạo use case mở khóa Tài khoản người dùng.
    /// </summary>
    /// <param name="userAccountRepository">Repository tài khoản người dùng.</param>
    public UnlockUserAccountUseCase(IUserAccountRepository userAccountRepository)
    {
        _userAccountRepository = userAccountRepository;
    }

    /// <summary>
    /// Xử lý yêu cầu mở khóa Tài khoản người dùng.
    /// </summary>
    /// <param name="id">Mã định danh tài khoản.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả mở khóa tài khoản.</returns>
    public async Task<IdentityAccessAppResult<UserAccountDto>> HandleAsync(Guid id, CancellationToken cancellationToken)
    {
        if (id == Guid.Empty)
        {
            return IdentityAccessAppResult<UserAccountDto>.Validation(new Dictionary<string, string[]>
            {
                ["id"] = ["Id is required."]
            });
        }

        var account = await _userAccountRepository.GetByIdAsync(id, cancellationToken);
        if (account is null || account.Status == UserAccountStatus.Deleted)
        {
            return IdentityAccessAppResult<UserAccountDto>.NotFound();
        }

        account.Unlock();
        await _userAccountRepository.SaveChangesAsync(cancellationToken);
        return IdentityAccessAppResult<UserAccountDto>.Ok(UserAccountMapping.ToDto(account));
    }
}
