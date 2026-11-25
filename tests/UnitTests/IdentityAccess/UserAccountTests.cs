using IdentityAccess.Domain.UserAccounts;

namespace UnitTests.IdentityAccess;

/// <summary>
/// Unit test cho thực thể Tài khoản người dùng.
/// </summary>
public sealed class UserAccountTests
{
    [Fact]
    public void Create_WithValidData_ReturnsActiveAccount()
    {
        var account = UserAccount.Create(Guid.NewGuid(), "admin", "admin@example.com", "Admin", UserRole.SystemAdministrator, "hash");

        Assert.Equal("admin", account.UserName);
        Assert.Equal("admin@example.com", account.Email);
        Assert.Equal(UserRole.SystemAdministrator, account.Role);
        Assert.Equal(UserAccountStatus.Active, account.Status);
        Assert.Equal("hash", account.PasswordHash);
    }

    [Fact]
    public void Create_WithEmptyUserName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            UserAccount.Create(Guid.NewGuid(), "", "admin@example.com", "Admin", UserRole.SystemAdministrator, "hash"));
    }
}
