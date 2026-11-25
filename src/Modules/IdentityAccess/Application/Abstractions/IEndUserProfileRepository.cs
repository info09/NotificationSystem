using IdentityAccess.Domain.EndUserProfiles;

namespace IdentityAccess.Application.Abstractions;

/// <summary>
/// Repository truy cập dữ liệu Hồ sơ người nhận EndUser.
/// </summary>
public interface IEndUserProfileRepository
{
    /// <summary>
    /// Lấy Hồ sơ người nhận EndUser theo mã định danh.
    /// </summary>
    /// <param name="id">Mã định danh hồ sơ.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Hồ sơ hoặc <c>null</c>.</returns>
    Task<EndUserProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Lấy Hồ sơ người nhận EndUser theo định danh hệ thống nguồn.
    /// </summary>
    /// <param name="externalUserId">Định danh hệ thống nguồn.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Hồ sơ hoặc <c>null</c>.</returns>
    Task<EndUserProfile?> GetByExternalUserIdAsync(string externalUserId, CancellationToken cancellationToken);

    /// <summary>
    /// Lấy danh sách Hồ sơ người nhận EndUser có phân trang.
    /// </summary>
    /// <param name="status">Trạng thái lọc.</param>
    /// <param name="externalUserId">Định danh hệ thống nguồn lọc.</param>
    /// <param name="page">Số trang.</param>
    /// <param name="pageSize">Kích thước trang.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Danh sách hồ sơ và tổng số bản ghi.</returns>
    Task<(IReadOnlyList<EndUserProfile> Items, int TotalCount)> GetPagedAsync(
        EndUserProfileStatus? status,
        string? externalUserId,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    /// <summary>
    /// Kiểm tra tồn tại định danh hệ thống nguồn.
    /// </summary>
    /// <param name="externalUserId">Định danh hệ thống nguồn.</param>
    /// <param name="excludeId">Mã định danh cần loại trừ.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns><c>true</c> nếu tồn tại; ngược lại <c>false</c>.</returns>
    Task<bool> ExistsByExternalUserIdAsync(string externalUserId, Guid? excludeId, CancellationToken cancellationToken);

    /// <summary>
    /// Thêm Hồ sơ người nhận EndUser.
    /// </summary>
    /// <param name="profile">Thực thể hồ sơ.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    Task AddAsync(EndUserProfile profile, CancellationToken cancellationToken);

    /// <summary>
    /// Lưu thay đổi dữ liệu.
    /// </summary>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
