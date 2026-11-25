using IdentityAccess.Domain.IntegrationClientCredentials;

namespace IdentityAccess.Application.Abstractions;

/// <summary>
/// Repository truy cập dữ liệu Thông tin xác thực IntegrationClient.
/// </summary>
public interface IIntegrationClientCredentialRepository
{
    /// <summary>
    /// Lấy credential theo mã định danh.
    /// </summary>
    /// <param name="id">Mã định danh credential.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Credential hoặc <c>null</c>.</returns>
    Task<IntegrationClientCredential?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Lấy danh sách credential theo Tài khoản người dùng.
    /// </summary>
    /// <param name="userAccountId">Mã định danh tài khoản.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Danh sách credential.</returns>
    Task<IReadOnlyList<IntegrationClientCredential>> GetByUserAccountIdAsync(Guid userAccountId, CancellationToken cancellationToken);

    /// <summary>
    /// Kiểm tra tồn tại ClientId.
    /// </summary>
    /// <param name="clientId">ClientId.</param>
    /// <param name="excludeId">Mã định danh cần loại trừ.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns><c>true</c> nếu tồn tại; ngược lại <c>false</c>.</returns>
    Task<bool> ExistsByClientIdAsync(string clientId, Guid? excludeId, CancellationToken cancellationToken);

    /// <summary>
    /// Thêm credential.
    /// </summary>
    /// <param name="credential">Thực thể credential.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    Task AddAsync(IntegrationClientCredential credential, CancellationToken cancellationToken);

    /// <summary>
    /// Lưu thay đổi dữ liệu.
    /// </summary>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
