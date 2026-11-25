using IdentityAccess.Domain.PasswordResetTokens;

namespace UnitTests.IdentityAccess;

/// <summary>
/// Unit test cho thực thể Mã đặt lại mật khẩu.
/// </summary>
public sealed class PasswordResetTokenTests
{
    [Fact]
    public void Create_WithValidData_ReturnsToken()
    {
        var now = DateTime.UtcNow;
        var token = PasswordResetToken.Create(Guid.NewGuid(), Guid.NewGuid(), "hash", now.AddMinutes(30), now);

        Assert.Equal("hash", token.TokenHash);
        Assert.Null(token.UsedAtUtc);
        Assert.True(token.ExpiresAtUtc > token.CreatedAtUtc);
    }

    [Fact]
    public void Create_WithInvalidExpiry_ThrowsArgumentException()
    {
        var now = DateTime.UtcNow;
        Assert.Throws<ArgumentException>(() =>
            PasswordResetToken.Create(Guid.NewGuid(), Guid.NewGuid(), "hash", now.AddMinutes(-1), now));
    }
}
