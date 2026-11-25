using IdentityAccess.Domain.EndUserProfiles;

namespace IdentityAccess.Application.EndUserProfiles;

/// <summary>
/// Cung cấp hàm ánh xạ cho Hồ sơ người nhận EndUser.
/// </summary>
internal static class EndUserProfileMapping
{
    /// <summary>
    /// Ánh xạ thực thể Hồ sơ người nhận EndUser sang DTO.
    /// </summary>
    /// <param name="profile">Thực thể hồ sơ.</param>
    /// <returns>DTO hồ sơ.</returns>
    public static EndUserProfileDto ToDto(EndUserProfile profile) =>
        new(
            profile.Id,
            profile.ExternalUserId,
            profile.DisplayName,
            profile.Email,
            profile.PhoneNumber,
            profile.Status.ToString(),
            profile.CreatedAtUtc,
            profile.UpdatedAtUtc);
}
