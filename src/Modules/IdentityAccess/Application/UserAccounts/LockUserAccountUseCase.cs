using IdentityAccess.Application.Abstractions;
using IdentityAccess.Domain.UserAccounts;

namespace IdentityAccess.Application.UserAccounts;

/// <summary>
/// Use case khóa Tài khoản người dùng.
/// </summary>
public sealed class LockUserAccountUseCase
{
    private readonly IUserAccountRepository _userAccountRepository;

    /// <summary>
    /// Khởi tạo use case khóa Tài khoản người dùng.
    /// </summary>
    /// <param name="userAccountRepository">Repository tài khoản người dùng.</param>
    public LockUserAccountUseCase(IUserAccountRepository userAccountRepository)
    {
        _userAccountRepository = userAccountRepository;
    }

    /// <summary>
    /// Xử lý yêu cầu khóa Tài khoản người dùng.
    /// </summary>
    /// <param name="id">Mã định danh tài khoản.</param>
    /// <param name="caller">Ngữ cảnh người gọi.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả khóa tài khoản.</returns>
    public async Task<IdentityAccessAppResult<UserAccountDto>> HandleAsync(
        Guid id,
        IdentityAccessCallerContext caller,
        CancellationToken cancellationToken)
    {
        if (id == Guid.Empty)
        {
            return IdentityAccessAppResult<UserAccountDto>.Validation(new Dictionary<string, string[]>
            {
                ["id"] = ["Id is required."]
            });
        }

        if (string.Equals(caller.Subject, id.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            return IdentityAccessAppResult<UserAccountDto>.Validation(new Dictionary<string, string[]>
            {
                ["id"] = ["Cannot lock the current account."]
            });
        }

        var account = await _userAccountRepository.GetByIdAsync(id, cancellationToken);
        if (account is null || account.Status == UserAccountStatus.Deleted)
        {
            return IdentityAccessAppResult<UserAccountDto>.NotFound();
        }

        account.Lock();
        await _userAccountRepository.SaveChangesAsync(cancellationToken);
        return IdentityAccessAppResult<UserAccountDto>.Ok(UserAccountMapping.ToDto(account));
    }
}
