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
/// Integration test cho API quản trị Hồ sơ người nhận EndUser.
/// </summary>
public sealed class EndUserProfileApiIntegrationTests : IAsyncLifetime
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

    /// <summary>
    /// Kiểm tra vòng đời CRUD/lock/unlock của Hồ sơ người nhận EndUser.
    /// </summary>
    [Fact]
    public async Task EndUserProfileLifecycle_WithSystemAdministrator_Works()
    {
        var createBody = new
        {
            externalUserId = "ext-user-1",
            displayName = "End User 1",
            email = "enduser1@example.com",
            phoneNumber = "0123456789"
        };

        using var createRequest = CreateAuthorizedJsonRequest(HttpMethod.Post, "/api/admin/end-user-profiles", RbacRoles.SystemAdministrator, "admin-1", createBody);
        var createResponse = await _client.SendAsync(createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<EndUserProfileResponse>();
        Assert.NotNull(created);
        Assert.Equal("ext-user-1", created!.ExternalUserId);

        using var getByExternalIdRequest = CreateAuthorizedRequest(HttpMethod.Get, "/api/admin/end-user-profiles/by-external-id/ext-user-1", RbacRoles.SystemAdministrator, "admin-1");
        var getByExternalIdResponse = await _client.SendAsync(getByExternalIdRequest);
        Assert.Equal(HttpStatusCode.OK, getByExternalIdResponse.StatusCode);

        var updateBody = new
        {
            displayName = "End User 1 Updated",
            email = "updated@example.com",
            phoneNumber = (string?)null
        };
        using var updateRequest = CreateAuthorizedJsonRequest(HttpMethod.Put, $"/api/admin/end-user-profiles/{created.Id}", RbacRoles.SystemAdministrator, "admin-1", updateBody);
        var updateResponse = await _client.SendAsync(updateRequest);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await updateResponse.Content.ReadFromJsonAsync<EndUserProfileResponse>();
        Assert.Equal("End User 1 Updated", updated!.DisplayName);

        using var lockRequest = CreateAuthorizedRequest(HttpMethod.Post, $"/api/admin/end-user-profiles/{created.Id}/lock", RbacRoles.SystemAdministrator, "admin-1");
        var lockResponse = await _client.SendAsync(lockRequest);
        Assert.Equal(HttpStatusCode.OK, lockResponse.StatusCode);
        var locked = await lockResponse.Content.ReadFromJsonAsync<EndUserProfileResponse>();
        Assert.Equal("Locked", locked!.Status);

        using var unlockRequest = CreateAuthorizedRequest(HttpMethod.Post, $"/api/admin/end-user-profiles/{created.Id}/unlock", RbacRoles.SystemAdministrator, "admin-1");
        var unlockResponse = await _client.SendAsync(unlockRequest);
        Assert.Equal(HttpStatusCode.OK, unlockResponse.StatusCode);
        var unlocked = await unlockResponse.Content.ReadFromJsonAsync<EndUserProfileResponse>();
        Assert.Equal("Active", unlocked!.Status);
    }

    /// <summary>
    /// Kiểm tra tạo trùng định danh nguồn trả về Conflict.
    /// </summary>
    [Fact]
    public async Task CreateEndUserProfile_WithDuplicateExternalUserId_ReturnsConflict()
    {
        var body = new { externalUserId = "dup-ext", displayName = "User", email = (string?)null, phoneNumber = (string?)null };

        using var first = CreateAuthorizedJsonRequest(HttpMethod.Post, "/api/admin/end-user-profiles", RbacRoles.SystemAdministrator, "admin-1", body);
        var firstResponse = await _client.SendAsync(first);
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        using var second = CreateAuthorizedJsonRequest(HttpMethod.Post, "/api/admin/end-user-profiles", RbacRoles.SystemAdministrator, "admin-1", body);
        var secondResponse = await _client.SendAsync(second);
        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
    }

    /// <summary>
    /// Kiểm tra truy cập không có quyền trả về Forbidden.
    /// </summary>
    [Fact]
    public async Task GetEndUserProfiles_WithBusinessOperator_ReturnsForbidden()
    {
        using var request = CreateAuthorizedRequest(HttpMethod.Get, "/api/admin/end-user-profiles", RbacRoles.BusinessOperator, "operator-1");
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
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

    private sealed record EndUserProfileResponse(Guid Id, string ExternalUserId, string DisplayName, string? Email, string? PhoneNumber, string Status, DateTime CreatedAtUtc, DateTime UpdatedAtUtc);
}
