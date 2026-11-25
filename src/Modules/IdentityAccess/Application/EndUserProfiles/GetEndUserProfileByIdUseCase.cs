using IdentityAccess.Application.Abstractions;

namespace IdentityAccess.Application.EndUserProfiles;

/// <summary>
/// Use case lấy chi tiết Hồ sơ người nhận EndUser.
/// </summary>
public sealed class GetEndUserProfileByIdUseCase
{
    private readonly IEndUserProfileRepository _endUserProfileRepository;

    /// <summary>
    /// Khởi tạo use case lấy chi tiết Hồ sơ người nhận EndUser.
    /// </summary>
    /// <param name="endUserProfileRepository">Repository hồ sơ EndUser.</param>
    public GetEndUserProfileByIdUseCase(IEndUserProfileRepository endUserProfileRepository)
    {
        _endUserProfileRepository = endUserProfileRepository;
    }

    /// <summary>
    /// Xử lý yêu cầu lấy chi tiết Hồ sơ người nhận EndUser.
    /// </summary>
    /// <param name="id">Mã định danh hồ sơ.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả truy vấn chi tiết.</returns>
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

        return IdentityAccessAppResult<EndUserProfileDto>.Ok(EndUserProfileMapping.ToDto(profile));
    }
}
