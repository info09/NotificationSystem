using IdentityAccess.Application.Abstractions;

namespace IdentityAccess.Application.EndUserProfiles;

/// <summary>
/// Use case cập nhật Hồ sơ người nhận EndUser.
/// </summary>
public sealed class UpdateEndUserProfileUseCase
{
    private readonly IEndUserProfileRepository _endUserProfileRepository;

    /// <summary>
    /// Khởi tạo use case cập nhật Hồ sơ người nhận EndUser.
    /// </summary>
    /// <param name="endUserProfileRepository">Repository hồ sơ EndUser.</param>
    public UpdateEndUserProfileUseCase(IEndUserProfileRepository endUserProfileRepository)
    {
        _endUserProfileRepository = endUserProfileRepository;
    }

    /// <summary>
    /// Xử lý yêu cầu cập nhật Hồ sơ người nhận EndUser.
    /// </summary>
    /// <param name="id">Mã định danh hồ sơ.</param>
    /// <param name="request">Yêu cầu cập nhật.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả cập nhật hồ sơ.</returns>
    public async Task<IdentityAccessAppResult<EndUserProfileDto>> HandleAsync(
        Guid id,
        UpdateEndUserProfileAppRequest? request,
        CancellationToken cancellationToken)
    {
        if (id == Guid.Empty)
        {
            return IdentityAccessAppResult<EndUserProfileDto>.Validation(new Dictionary<string, string[]>
            {
                ["id"] = ["Id is required."]
            });
        }

        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        if (request is null)
        {
            errors["request"] = ["Request body is required."];
            return IdentityAccessAppResult<EndUserProfileDto>.Validation(errors);
        }

        var displayName = request.DisplayName?.Trim();
        if (string.IsNullOrWhiteSpace(displayName))
        {
            errors["displayName"] = ["DisplayName is required."];
        }

        if (errors.Count > 0)
        {
            return IdentityAccessAppResult<EndUserProfileDto>.Validation(errors);
        }

        var profile = await _endUserProfileRepository.GetByIdAsync(id, cancellationToken);
        if (profile is null)
        {
            return IdentityAccessAppResult<EndUserProfileDto>.NotFound();
        }

        profile.Update(displayName!, request.Email, request.PhoneNumber);
        await _endUserProfileRepository.SaveChangesAsync(cancellationToken);

        return IdentityAccessAppResult<EndUserProfileDto>.Ok(EndUserProfileMapping.ToDto(profile));
    }
}
