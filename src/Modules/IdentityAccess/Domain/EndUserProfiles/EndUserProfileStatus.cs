namespace IdentityAccess.Domain.EndUserProfiles;

/// <summary>
/// Trạng thái Hồ sơ người nhận EndUser.
/// </summary>
public enum EndUserProfileStatus
{
    /// <summary>
    /// Hồ sơ đang hoạt động.
    /// </summary>
    Active = 1,

    /// <summary>
    /// Hồ sơ bị khóa.
    /// </summary>
    Locked = 2
}
