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
/// Integration test cho API quản trị Tài khoản người dùng.
/// </summary>
public sealed class UserAccountManagementApiIntegrationTests : IAsyncLifetime
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
    public async Task GetUsers_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/admin/users");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetUsers_WithBusinessOperator_ReturnsForbidden()
    {
        using var request = CreateAuthorizedRequest(HttpMethod.Get, "/api/admin/users", RbacRoles.BusinessOperator, "operator-1");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UserLifecycle_WithSystemAdministrator_Works()
    {
        var createRequestBody = new
        {
            userName = "admin.user",
            email = "admin.user@example.com",
            displayName = "Admin User",
            role = "SystemAdministrator",
            password = "Password123!"
        };

        using var createRequest = CreateAuthorizedJsonRequest(HttpMethod.Post, "/api/admin/users", RbacRoles.SystemAdministrator, "admin-1", createRequestBody);
        var createResponse = await _client.SendAsync(createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<UserAccountResponse>();
        Assert.NotNull(created);
        Assert.Equal("admin.user", created!.UserName);

        using var getRequest = CreateAuthorizedRequest(HttpMethod.Get, $"/api/admin/users/{created.Id}", RbacRoles.SystemAdministrator, "admin-1");
        var getResponse = await _client.SendAsync(getRequest);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var updateBody = new
        {
            userName = "admin.user2",
            email = "admin.user2@example.com",
            displayName = "Admin User 2",
            role = "BusinessOperator",
            password = "NewPassword123!"
        };
        using var updateRequest = CreateAuthorizedJsonRequest(HttpMethod.Put, $"/api/admin/users/{created.Id}", RbacRoles.SystemAdministrator, "admin-1", updateBody);
        var updateResponse = await _client.SendAsync(updateRequest);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await updateResponse.Content.ReadFromJsonAsync<UserAccountResponse>();
        Assert.NotNull(updated);
        Assert.Equal("BusinessOperator", updated!.Role);

        using var lockRequest = CreateAuthorizedRequest(HttpMethod.Post, $"/api/admin/users/{created.Id}/lock", RbacRoles.SystemAdministrator, "admin-1");
        var lockResponse = await _client.SendAsync(lockRequest);
        Assert.Equal(HttpStatusCode.OK, lockResponse.StatusCode);
        var locked = await lockResponse.Content.ReadFromJsonAsync<UserAccountResponse>();
        Assert.Equal("Locked", locked!.Status);

        using var unlockRequest = CreateAuthorizedRequest(HttpMethod.Post, $"/api/admin/users/{created.Id}/unlock", RbacRoles.SystemAdministrator, "admin-1");
        var unlockResponse = await _client.SendAsync(unlockRequest);
        Assert.Equal(HttpStatusCode.OK, unlockResponse.StatusCode);

        using var deleteRequest = CreateAuthorizedRequest(HttpMethod.Delete, $"/api/admin/users/{created.Id}", RbacRoles.SystemAdministrator, "admin-1");
        var deleteResponse = await _client.SendAsync(deleteRequest);
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
        var deleted = await deleteResponse.Content.ReadFromJsonAsync<UserAccountResponse>();
        Assert.Equal("Deleted", deleted!.Status);
    }

    [Fact]
    public async Task CreateCredential_ForIntegrationClient_ReturnsSecretOnce()
    {
        var createUserBody = new
        {
            userName = "client.user",
            email = "client.user@example.com",
            displayName = "Client User",
            role = "IntegrationClient",
            password = (string?)null
        };

        using var createUserRequest = CreateAuthorizedJsonRequest(HttpMethod.Post, "/api/admin/users", RbacRoles.SystemAdministrator, "admin-1", createUserBody);
        var createUserResponse = await _client.SendAsync(createUserRequest);
        Assert.Equal(HttpStatusCode.Created, createUserResponse.StatusCode);
        var user = await createUserResponse.Content.ReadFromJsonAsync<UserAccountResponse>();

        using var createCredentialRequest = CreateAuthorizedJsonRequest(HttpMethod.Post, $"/api/admin/users/{user!.Id}/credentials", RbacRoles.SystemAdministrator, "admin-1", new { clientId = "client-1" });
        var createCredentialResponse = await _client.SendAsync(createCredentialRequest);
        Assert.Equal(HttpStatusCode.Created, createCredentialResponse.StatusCode);
        var credential = await createCredentialResponse.Content.ReadFromJsonAsync<CreatedCredentialResponse>();
        Assert.NotNull(credential);
        Assert.False(string.IsNullOrWhiteSpace(credential!.ClientSecret));
        Assert.Equal("client-1", credential.Credential.ClientId);
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

    private sealed record UserAccountResponse(Guid Id, string UserName, string Email, string DisplayName, string Role, string Status, DateTime CreatedAtUtc, DateTime UpdatedAtUtc);
    private sealed record IntegrationClientCredentialResponse(Guid Id, Guid UserAccountId, string ClientId, string Status, DateTime CreatedAtUtc, DateTime? ExpiresAtUtc, DateTime? RevokedAtUtc, DateTime? LastRotatedAtUtc);
    private sealed record CreatedCredentialResponse(IntegrationClientCredentialResponse Credential, string ClientSecret);
}
