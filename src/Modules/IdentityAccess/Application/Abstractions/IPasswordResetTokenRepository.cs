using IdentityAccess.Domain.PasswordResetTokens;

namespace IdentityAccess.Application.Abstractions;

/// <summary>
/// Repository truy cập dữ liệu Mã đặt lại mật khẩu.
/// </summary>
public interface IPasswordResetTokenRepository
{
    /// <summary>
    /// Thêm Mã đặt lại mật khẩu.
    /// </summary>
    /// <param name="token">Thực thể token.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    Task AddAsync(PasswordResetToken token, CancellationToken cancellationToken);

    /// <summary>
    /// Lấy Mã đặt lại mật khẩu theo giá trị băm.
    /// </summary>
    /// <param name="tokenHash">Giá trị băm token.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Thực thể token hoặc <c>null</c>.</returns>
    Task<PasswordResetToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken);

    /// <summary>
    /// Lấy danh sách token đang hoạt động của Tài khoản người dùng.
    /// </summary>
    /// <param name="userAccountId">Mã định danh tài khoản.</param>
    /// <param name="utcNow">Thời điểm hiện tại UTC.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Danh sách token đang hoạt động.</returns>
    Task<IReadOnlyList<PasswordResetToken>> GetActiveByUserAccountIdAsync(Guid userAccountId, DateTime utcNow, CancellationToken cancellationToken);

    /// <summary>
    /// Lưu thay đổi dữ liệu.
    /// </summary>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
