using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Delivery.Infrastructure.Persistence;
using IdentityAccess.Domain;
using IdentityAccess.Infrastructure.Persistence;
using IntegrationMessaging.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Notification.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace IntegrationTests.IdentityAccess;

/// <summary>
/// Integration test cho endpoint cấp JWT bằng client credentials.
/// </summary>
public sealed class ClientCredentialsTokenApiIntegrationTests : IAsyncLifetime
{
    private const string Issuer = "test-issuer";
    private const string Audience = "test-audience";
    private const string SigningKey = "integration-test-signing-key-min-32-chars-0001";

    private readonly PostgreSqlContainer _postgreSqlContainer = TestcontainersConfiguration.CreatePostgreSqlBuilder().Build();
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    /// <summary>
    /// Khởi tạo môi trường test.
    /// </summary>
    public async Task InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", _postgreSqlContainer.GetConnectionString());
            builder.UseSetting("JWT_ISSUER", Issuer);
            builder.UseSetting("JWT_AUDIENCE", Audience);
            builder.UseSetting("JWT_SIGNING_KEY", SigningKey);
        });

        using (var scope = _factory.Services.CreateScope())
        {
            await scope.ServiceProvider.GetRequiredService<NotificationDbContext>().Database.MigrateAsync();
            await scope.ServiceProvider.GetRequiredService<IntegrationMessagingDbContext>().Database.MigrateAsync();
            await scope.ServiceProvider.GetRequiredService<DeliveryDbContext>().Database.MigrateAsync();
            await scope.ServiceProvider.GetRequiredService<IdentityAccessDbContext>().Database.MigrateAsync();
        }

        _client = _factory.CreateClient();
    }

    /// <summary>
    /// Giải phóng tài nguyên test.
    /// </summary>
    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
        await _postgreSqlContainer.DisposeAsync();
    }

    [Fact]
    public async Task IssueToken_WithValidClientCredentials_ReturnsJwt()
    {
        var (clientId, clientSecret) = await CreateIntegrationClientWithCredentialAsync();

        var response = await _client.PostAsJsonAsync("/api/auth/client-credentials/token", new { clientId, clientSecret });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<TokenResponse>();
        Assert.NotNull(payload);
        Assert.Equal("Bearer", payload!.TokenType);
        Assert.False(string.IsNullOrWhiteSpace(payload.AccessToken));

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(payload.AccessToken);
        Assert.Equal(Issuer, jwt.Issuer);
        Assert.Contains(Audience, jwt.Audiences);
        Assert.Contains(jwt.Claims, c => (c.Type == ClaimTypes.Role || c.Type == "role") && c.Value == RbacRoles.IntegrationClient);
        Assert.Contains(jwt.Claims, c => c.Type == "client_id" && c.Value == clientId);
    }

    [Fact]
    public async Task IssueToken_WithInvalidSecret_ReturnsUnauthorized()
    {
        var (clientId, _) = await CreateIntegrationClientWithCredentialAsync();

        var response = await _client.PostAsJsonAsync("/api/auth/client-credentials/token", new { clientId, clientSecret = "wrong-secret" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private async Task<(string ClientId, string ClientSecret)> CreateIntegrationClientWithCredentialAsync()
    {
        using var createUserRequest = new HttpRequestMessage(HttpMethod.Post, "/api/admin/users");
        createUserRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", CreateAdminToken());
        createUserRequest.Content = JsonContent.Create(new
        {
            userName = $"client-{Guid.NewGuid():N}",
            email = $"client-{Guid.NewGuid():N}@example.com",
            displayName = "Integration Client",
            role = "IntegrationClient",
            password = (string?)null
        });
        var createUserResponse = await _client.SendAsync(createUserRequest);
        createUserResponse.EnsureSuccessStatusCode();
        var user = await createUserResponse.Content.ReadFromJsonAsync<UserResponse>();

        using var createCredentialRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/admin/users/{user!.Id}/credentials");
        createCredentialRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", CreateAdminToken());
        createCredentialRequest.Content = JsonContent.Create(new { clientId = $"cid-{Guid.NewGuid():N}" });
        var createCredentialResponse = await _client.SendAsync(createCredentialRequest);
        createCredentialResponse.EnsureSuccessStatusCode();
        var credential = await createCredentialResponse.Content.ReadFromJsonAsync<CreatedCredentialResponse>();

        return (credential!.Credential.ClientId, credential.ClientSecret);
    }

    private static string CreateAdminToken()
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims:
            [
                new Claim(ClaimTypes.NameIdentifier, "admin-1"),
                new Claim(ClaimTypes.Role, RbacRoles.SystemAdministrator)
            ],
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private sealed record UserResponse(Guid Id);
    private sealed record CredentialResponse(string ClientId);
    private sealed record CreatedCredentialResponse(CredentialResponse Credential, string ClientSecret);
    private sealed record TokenResponse(string AccessToken, string TokenType, int ExpiresIn);
}
