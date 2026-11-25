using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using IdentityAccess.Infrastructure.Security;
using Microsoft.Extensions.Configuration;

namespace UnitTests.IdentityAccess;

/// <summary>
/// Unit test cho dịch vụ phát hành JWT.
/// </summary>
public sealed class JwtTokenIssuerTests
{
    [Fact]
    public void IssueAccessToken_WithValidConfig_ReturnsJwtWithExpectedClaims()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JWT_ISSUER"] = "test-issuer",
                ["JWT_AUDIENCE"] = "test-audience",
                ["JWT_SIGNING_KEY"] = "test-signing-key-min-32-chars-0000000001",
                ["JWT_ACCESS_TOKEN_MINUTES"] = "60"
            })
            .Build();
        var issuer = new JwtTokenIssuer(configuration);

        var result = issuer.IssueAccessToken("user-1", "IntegrationClient", new Dictionary<string, string> { ["client_id"] = "client-1" });

        Assert.Equal("Bearer", result.TokenType);
        Assert.Equal(3600, result.ExpiresInSeconds);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(result.AccessToken);
        Assert.Equal("test-issuer", jwt.Issuer);
        Assert.Contains("test-audience", jwt.Audiences);
        Assert.Contains(jwt.Claims, c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == "user-1");
        Assert.Contains(jwt.Claims, c => (c.Type == ClaimTypes.Role || c.Type == "role") && c.Value == "IntegrationClient");
        Assert.Contains(jwt.Claims, c => c.Type == "client_id" && c.Value == "client-1");
    }
}
