using IdentityAccess.Infrastructure.Security;

namespace UnitTests.IdentityAccess;

/// <summary>
/// Unit test cho dịch vụ băm credential PBKDF2.
/// </summary>
public sealed class Pbkdf2CredentialHasherTests
{
    [Fact]
    public void Hash_And_Verify_WithSamePlainText_ReturnsTrue()
    {
        var hasher = new Pbkdf2CredentialHasher();

        var hash = hasher.Hash("Password123!");

        Assert.True(hasher.Verify(hash, "Password123!"));
        Assert.False(hasher.Verify(hash, "wrong-password"));
    }

    [Fact]
    public void Hash_WithSameInput_ProducesDifferentHashes()
    {
        var hasher = new Pbkdf2CredentialHasher();

        var hash1 = hasher.Hash("Password123!");
        var hash2 = hasher.Hash("Password123!");

        Assert.NotEqual(hash1, hash2);
    }
}
