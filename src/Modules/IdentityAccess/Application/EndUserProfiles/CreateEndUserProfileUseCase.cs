using IdentityAccess.Application.Abstractions;
using IdentityAccess.Domain.EndUserProfiles;

namespace IdentityAccess.Application.EndUserProfiles;

/// <summary>
/// Use case tạo Hồ sơ người nhận EndUser.
/// </summary>
public sealed class CreateEndUserProfileUseCase
{
    private readonly IEndUserProfileRepository _endUserProfileRepository;

    /// <summary>
    /// Khởi tạo use case tạo Hồ sơ người nhận EndUser.
    /// </summary>
    /// <param name="endUserProfileRepository">Repository hồ sơ EndUser.</param>
    public CreateEndUserProfileUseCase(IEndUserProfileRepository endUserProfileRepository)
    {
        _endUserProfileRepository = endUserProfileRepository;
    }

    /// <summary>
    /// Xử lý yêu cầu tạo Hồ sơ người nhận EndUser.
    /// </summary>
    /// <param name="request">Yêu cầu tạo hồ sơ.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả tạo hồ sơ.</returns>
    public async Task<IdentityAccessAppResult<EndUserProfileDto>> HandleAsync(
        CreateEndUserProfileAppRequest? request,
        CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        if (request is null)
        {
            errors["request"] = ["Request body is required."];
            return IdentityAccessAppResult<EndUserProfileDto>.Validation(errors);
        }

        var externalUserId = request.ExternalUserId?.Trim();
        var displayName = request.DisplayName?.Trim();

        if (string.IsNullOrWhiteSpace(externalUserId))
        {
            errors["externalUserId"] = ["ExternalUserId is required."];
        }

        if (string.IsNullOrWhiteSpace(displayName))
        {
            errors["displayName"] = ["DisplayName is required."];
        }

        if (errors.Count > 0)
        {
            return IdentityAccessAppResult<EndUserProfileDto>.Validation(errors);
        }

        if (await _endUserProfileRepository.ExistsByExternalUserIdAsync(externalUserId!, null, cancellationToken))
        {
            return IdentityAccessAppResult<EndUserProfileDto>.Conflict();
        }

        var profile = EndUserProfile.Create(
            Guid.NewGuid(),
            externalUserId!,
            displayName!,
            request.Email,
            request.PhoneNumber);

        await _endUserProfileRepository.AddAsync(profile, cancellationToken);
        await _endUserProfileRepository.SaveChangesAsync(cancellationToken);

        return IdentityAccessAppResult<EndUserProfileDto>.Created(EndUserProfileMapping.ToDto(profile));
    }
}
