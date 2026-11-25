using IdentityAccess.Domain.UserAccounts;

namespace IdentityAccess.Application.Abstractions;

/// <summary>
/// Repository truy cập dữ liệu Tài khoản người dùng.
/// </summary>
public interface IUserAccountRepository
{
    /// <summary>
    /// Lấy Tài khoản người dùng theo mã định danh.
    /// </summary>
    /// <param name="id">Mã định danh tài khoản.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Tài khoản người dùng hoặc <c>null</c>.</returns>
    Task<UserAccount?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Lấy Tài khoản người dùng theo tên đăng nhập.
    /// </summary>
    /// <param name="userName">Tên đăng nhập.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Tài khoản người dùng hoặc <c>null</c>.</returns>
    Task<UserAccount?> GetByUserNameAsync(string userName, CancellationToken cancellationToken);

    /// <summary>
    /// Lấy danh sách Tài khoản người dùng có phân trang.
    /// </summary>
    /// <param name="status">Trạng thái cần lọc.</param>
    /// <param name="role">Vai trò cần lọc.</param>
    /// <param name="page">Số trang.</param>
    /// <param name="pageSize">Kích thước trang.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Danh sách tài khoản và tổng số bản ghi.</returns>
    Task<(IReadOnlyList<UserAccount> Items, int TotalCount)> GetPagedAsync(
        UserAccountStatus? status,
        UserRole? role,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    /// <summary>
    /// Kiểm tra tồn tại tên đăng nhập.
    /// </summary>
    /// <param name="userName">Tên đăng nhập.</param>
    /// <param name="excludeId">Mã định danh cần loại trừ.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns><c>true</c> nếu tồn tại; ngược lại <c>false</c>.</returns>
    Task<bool> ExistsByUserNameAsync(string userName, Guid? excludeId, CancellationToken cancellationToken);

    /// <summary>
    /// Kiểm tra tồn tại email.
    /// </summary>
    /// <param name="email">Email.</param>
    /// <param name="excludeId">Mã định danh cần loại trừ.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns><c>true</c> nếu tồn tại; ngược lại <c>false</c>.</returns>
    Task<bool> ExistsByEmailAsync(string email, Guid? excludeId, CancellationToken cancellationToken);

    /// <summary>
    /// Thêm Tài khoản người dùng.
    /// </summary>
    /// <param name="account">Thực thể tài khoản.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    Task AddAsync(UserAccount account, CancellationToken cancellationToken);

    /// <summary>
    /// Lưu thay đổi dữ liệu.
    /// </summary>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
