using IdentityAccess.Application.Abstractions;

namespace IdentityAccess.Application.EndUserProfiles;

/// <summary>
/// Use case tra cứu Hồ sơ người nhận EndUser theo định danh hệ thống nguồn.
/// </summary>
public sealed class GetEndUserProfileByExternalUserIdUseCase
{
    private readonly IEndUserProfileRepository _endUserProfileRepository;

    /// <summary>
    /// Khởi tạo use case tra cứu Hồ sơ người nhận EndUser theo định danh hệ thống nguồn.
    /// </summary>
    /// <param name="endUserProfileRepository">Repository hồ sơ EndUser.</param>
    public GetEndUserProfileByExternalUserIdUseCase(IEndUserProfileRepository endUserProfileRepository)
    {
        _endUserProfileRepository = endUserProfileRepository;
    }

    /// <summary>
    /// Xử lý yêu cầu tra cứu Hồ sơ người nhận EndUser theo định danh hệ thống nguồn.
    /// </summary>
    /// <param name="externalUserId">Định danh hệ thống nguồn.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả truy vấn hồ sơ.</returns>
    public async Task<IdentityAccessAppResult<EndUserProfileDto>> HandleAsync(string? externalUserId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(externalUserId))
        {
            return IdentityAccessAppResult<EndUserProfileDto>.Validation(new Dictionary<string, string[]>
            {
                ["externalUserId"] = ["ExternalUserId is required."]
            });
        }

        var profile = await _endUserProfileRepository.GetByExternalUserIdAsync(externalUserId.Trim(), cancellationToken);
        if (profile is null)
        {
            return IdentityAccessAppResult<EndUserProfileDto>.NotFound();
        }

        return IdentityAccessAppResult<EndUserProfileDto>.Ok(EndUserProfileMapping.ToDto(profile));
    }
}
