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
}
