namespace BuildingBlocks.SharedKernel.EndUserProfiles;

/// <summary>
/// Cổng tham chiếu Hồ sơ người nhận EndUser cho module khác theo nghiệp vụ.
/// </summary>
public interface IEndUserProfileReferenceProvider
{
    /// <summary>
    /// Lấy thông tin tham chiếu Hồ sơ người nhận EndUser theo định danh hệ thống nguồn.
    /// </summary>
    /// <param name="externalUserId">Định danh hệ thống nguồn.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Thông tin tham chiếu hoặc <c>null</c> nếu không tìm thấy.</returns>
    Task<EndUserProfileReference?> GetByExternalUserIdAsync(string externalUserId, CancellationToken cancellationToken);
}
