using BuildingBlocks.SharedKernel.EndUserProfiles;
using IdentityAccess.Application.Abstractions;

namespace IdentityAccess.Infrastructure.EndUserProfiles;

/// <summary>
/// Cung cấp thông tin tham chiếu Hồ sơ người nhận EndUser cho module khác.
/// </summary>
public sealed class EndUserProfileReferenceProvider : IEndUserProfileReferenceProvider
{
    private readonly IEndUserProfileRepository _endUserProfileRepository;

    /// <summary>
    /// Khởi tạo provider tham chiếu Hồ sơ người nhận EndUser.
    /// </summary>
    /// <param name="endUserProfileRepository">Repository hồ sơ EndUser.</param>
    public EndUserProfileReferenceProvider(IEndUserProfileRepository endUserProfileRepository)
    {
        _endUserProfileRepository = endUserProfileRepository;
    }

    /// <inheritdoc />
    public async Task<EndUserProfileReference?> GetByExternalUserIdAsync(string externalUserId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(externalUserId))
        {
            return null;
        }

        var profile = await _endUserProfileRepository.GetByExternalUserIdAsync(externalUserId.Trim(), cancellationToken);
        if (profile is null)
        {
            return null;
        }

        return new EndUserProfileReference(
            profile.ExternalUserId,
            profile.DisplayName,
            profile.Status.ToString(),
            profile.Email,
            profile.PhoneNumber);
    }
}
