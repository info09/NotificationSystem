using IdentityAccess.Domain.EndUserProfiles;

namespace UnitTests.IdentityAccess;

/// <summary>
/// Unit test cho thực thể Hồ sơ người nhận EndUser.
/// </summary>
public sealed class EndUserProfileTests
{
    [Fact]
    public void Create_WithValidData_ReturnsActiveProfile()
    {
        var profile = EndUserProfile.Create(Guid.NewGuid(), "ext-1", "User 1", "u1@example.com", "0123");

        Assert.Equal("ext-1", profile.ExternalUserId);
        Assert.Equal("User 1", profile.DisplayName);
        Assert.Equal(EndUserProfileStatus.Active, profile.Status);
    }

    [Fact]
    public void Create_WithEmptyExternalUserId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            EndUserProfile.Create(Guid.NewGuid(), "", "User 1", null, null));
    }

    /// <summary>
    /// Kiểm tra cập nhật hồ sơ hợp lệ.
    /// </summary>
    [Fact]
    public void Update_WithValidData_UpdatesFieldsAndTimestamp()
    {
        var profile = EndUserProfile.Create(Guid.NewGuid(), "ext-1", "User 1", "u1@example.com", "0123");
        var updatedAt = DateTime.UtcNow.AddMinutes(1);

        profile.Update("User 1 Updated", " new@example.com ", " ", updatedAt);

        Assert.Equal("User 1 Updated", profile.DisplayName);
        Assert.Equal("new@example.com", profile.Email);
        Assert.Null(profile.PhoneNumber);
        Assert.Equal(updatedAt, profile.UpdatedAtUtc);
    }

    /// <summary>
    /// Kiểm tra khóa và mở khóa hồ sơ.
    /// </summary>
    [Fact]
    public void LockAndUnlock_ChangesStatus()
    {
        var profile = EndUserProfile.Create(Guid.NewGuid(), "ext-1", "User 1", null, null);

        profile.Lock();
        Assert.Equal(EndUserProfileStatus.Locked, profile.Status);

        profile.Unlock();
        Assert.Equal(EndUserProfileStatus.Active, profile.Status);
    }
}
