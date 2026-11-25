using Delivery.Infrastructure.Persistence;
using IdentityAccess.Domain;
using IdentityAccess.Infrastructure.Persistence;
using IntegrationMessaging.Infrastructure.Persistence;
using IntegrationTests;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Notification.Infrastructure.Persistence;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Testcontainers.PostgreSql;

namespace E2ETests.IdentityAccess;

/// <summary>
/// E2E test cho luồng quản trị tài khoản người dùng và credential IntegrationClient.
/// </summary>
public sealed class UserAccountManagementE2ETests : IAsyncLifetime
{
    private const string Issuer = "e2e-issuer";
    private const string Audience = "e2e-audience";
    private const string SigningKey = "e2e-test-signing-key-min-32-chars-000001";

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

    /// <summary>
    /// Kiểm tra luồng tạo tài khoản IntegrationClient, tạo credential và lấy token client credentials.
    /// </summary>
    [Fact]
    public async Task AdminCanCreateIntegrationClientAndIssueClientCredentialsToken()
    {
        var createUserBody = new
        {
            userName = "e2e-client",
            email = "e2e-client@example.com",
            displayName = "E2E Client",
            role = "IntegrationClient",
            password = (string?)null
        };

        using var createUserRequest = CreateAuthorizedJsonRequest(HttpMethod.Post, "/api/admin/users", RbacRoles.SystemAdministrator, "admin-1", createUserBody);
        var createUserResponse = await _client.SendAsync(createUserRequest);
        Assert.Equal(HttpStatusCode.Created, createUserResponse.StatusCode);
        var user = await createUserResponse.Content.ReadFromJsonAsync<UserAccountResponse>();
        Assert.NotNull(user);

        using var createCredentialRequest = CreateAuthorizedJsonRequest(HttpMethod.Post, $"/api/admin/users/{user!.Id}/credentials", RbacRoles.SystemAdministrator, "admin-1", new { clientId = "e2e-client-id" });
        var createCredentialResponse = await _client.SendAsync(createCredentialRequest);
        Assert.Equal(HttpStatusCode.Created, createCredentialResponse.StatusCode);
        var credential = await createCredentialResponse.Content.ReadFromJsonAsync<CreatedCredentialResponse>();
        Assert.NotNull(credential);
        Assert.False(string.IsNullOrWhiteSpace(credential!.ClientSecret));

        var tokenResponse = await _client.PostAsJsonAsync("/api/auth/client-credentials/token", new { clientId = "e2e-client-id", clientSecret = credential.ClientSecret });
        Assert.Equal(HttpStatusCode.OK, tokenResponse.StatusCode);
        var tokenPayload = await tokenResponse.Content.ReadFromJsonAsync<TokenResponse>();
        Assert.NotNull(tokenPayload);
        Assert.False(string.IsNullOrWhiteSpace(tokenPayload!.AccessToken));
    }

    private static HttpRequestMessage CreateAuthorizedRequest(HttpMethod method, string url, string role, string subject)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", CreateToken(role, subject));
        return request;
    }

    private static HttpRequestMessage CreateAuthorizedJsonRequest(HttpMethod method, string url, string role, string subject, object body)
    {
        var request = CreateAuthorizedRequest(method, url, role, subject);
        request.Content = JsonContent.Create(body);
        return request;
    }

    private static string CreateToken(string role, string subject)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims:
            [
                new Claim(ClaimTypes.NameIdentifier, subject),
                new Claim(ClaimTypes.Role, role)
            ],
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private sealed record UserAccountResponse(Guid Id, string UserName, string Email, string DisplayName, string Role, string Status);
    private sealed record IntegrationClientCredentialResponse(Guid Id, Guid UserAccountId, string ClientId, string Status);
    private sealed record CreatedCredentialResponse(IntegrationClientCredentialResponse Credential, string ClientSecret);
    private sealed record TokenResponse(string AccessToken, string TokenType, int ExpiresIn);
}
