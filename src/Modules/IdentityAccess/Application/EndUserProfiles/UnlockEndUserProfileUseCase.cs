using IdentityAccess.Application.Abstractions;

namespace IdentityAccess.Application.EndUserProfiles;

/// <summary>
/// Use case mở khóa Hồ sơ người nhận EndUser.
/// </summary>
public sealed class UnlockEndUserProfileUseCase
{
    private readonly IEndUserProfileRepository _endUserProfileRepository;

    /// <summary>
    /// Khởi tạo use case mở khóa Hồ sơ người nhận EndUser.
    /// </summary>
    /// <param name="endUserProfileRepository">Repository hồ sơ EndUser.</param>
    public UnlockEndUserProfileUseCase(IEndUserProfileRepository endUserProfileRepository)
    {
        _endUserProfileRepository = endUserProfileRepository;
    }

    /// <summary>
    /// Xử lý yêu cầu mở khóa Hồ sơ người nhận EndUser.
    /// </summary>
    /// <param name="id">Mã định danh hồ sơ.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả mở khóa hồ sơ.</returns>
    public async Task<IdentityAccessAppResult<EndUserProfileDto>> HandleAsync(Guid id, CancellationToken cancellationToken)
    {
        if (id == Guid.Empty)
        {
            return IdentityAccessAppResult<EndUserProfileDto>.Validation(new Dictionary<string, string[]>
            {
                ["id"] = ["Id is required."]
            });
        }

        var profile = await _endUserProfileRepository.GetByIdAsync(id, cancellationToken);
        if (profile is null)
        {
            return IdentityAccessAppResult<EndUserProfileDto>.NotFound();
        }

        profile.Unlock();
        await _endUserProfileRepository.SaveChangesAsync(cancellationToken);

        return IdentityAccessAppResult<EndUserProfileDto>.Ok(EndUserProfileMapping.ToDto(profile));
    }
}
