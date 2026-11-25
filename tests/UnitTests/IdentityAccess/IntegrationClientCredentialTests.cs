using IdentityAccess.Domain.IntegrationClientCredentials;

namespace UnitTests.IdentityAccess;

/// <summary>
/// Unit test cho thực thể Thông tin xác thực IntegrationClient.
/// </summary>
public sealed class IntegrationClientCredentialTests
{
    [Fact]
    public void Create_WithValidData_ReturnsActiveCredential()
    {
        var credential = IntegrationClientCredential.Create(Guid.NewGuid(), Guid.NewGuid(), "client-1", "secret-hash");

        Assert.Equal("client-1", credential.ClientId);
        Assert.Equal("secret-hash", credential.ClientSecretHash);
        Assert.Equal(IntegrationClientCredentialStatus.Active, credential.Status);
    }

    [Fact]
    public void Create_WithEmptyClientId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            IntegrationClientCredential.Create(Guid.NewGuid(), Guid.NewGuid(), "", "secret-hash"));
    }
}
