using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
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
using TemplateConfiguration.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace IntegrationTests.TemplateConfiguration;

/// <summary>
/// Integration test cho API quản trị Cấu hình kênh thông báo.
/// </summary>
public sealed class NotificationChannelConfigApiIntegrationTests : IAsyncLifetime
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
            await scope.ServiceProvider.GetRequiredService<TemplateConfigurationDbContext>().Database.MigrateAsync();
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
    /// Kiểm tra vòng đời CRUD của Cấu hình kênh thông báo và không lộ secret.
    /// </summary>
    [Fact]
    public async Task NotificationChannelConfigLifecycle_WithSystemAdministrator_Works()
    {
        var createBody = new
        {
            channel = "Email",
            displayName = "Email",
            description = "Cấu hình kênh Email",
            isEnabled = true
        };

        using var createRequest = CreateAuthorizedJsonRequest(HttpMethod.Post, "/api/admin/notification-channel-configs", RbacRoles.SystemAdministrator, "admin-1", createBody);
        var createResponse = await _client.SendAsync(createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var rawJson = await createResponse.Content.ReadAsStringAsync();
        Assert.DoesNotContain("secret", rawJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("password", rawJson, StringComparison.OrdinalIgnoreCase);

        var created = JsonSerializer.Deserialize<NotificationChannelConfigResponse>(rawJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(created);

        using var updateRequest = CreateAuthorizedJsonRequest(HttpMethod.Put, $"/api/admin/notification-channel-configs/{created!.Id}", RbacRoles.SystemAdministrator, "admin-1", new
        {
            displayName = "Email Updated",
            description = "Mô tả mới",
            isEnabled = false
        });
        var updateResponse = await _client.SendAsync(updateRequest);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        using var deleteRequest = CreateAuthorizedRequest(HttpMethod.Delete, $"/api/admin/notification-channel-configs/{created.Id}", RbacRoles.SystemAdministrator, "admin-1");
        var deleteResponse = await _client.SendAsync(deleteRequest);
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
    }

    /// <summary>
    /// Kiểm tra truy cập không có quyền trả về Forbidden.
    /// </summary>
    [Fact]
    public async Task GetNotificationChannelConfigs_WithBusinessOperator_ReturnsForbidden()
    {
        using var request = CreateAuthorizedRequest(HttpMethod.Get, "/api/admin/notification-channel-configs", RbacRoles.BusinessOperator, "operator-1");
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    /// <summary>
    /// Kiểm tra tạo trùng kênh trả về Conflict.
    /// </summary>
    [Fact]
    public async Task CreateNotificationChannelConfig_WithDuplicateChannel_ReturnsConflict()
    {
        var body = new { channel = "Email", displayName = "Email", description = (string?)null, isEnabled = true };

        using var first = CreateAuthorizedJsonRequest(HttpMethod.Post, "/api/admin/notification-channel-configs", RbacRoles.SystemAdministrator, "admin-1", body);
        var firstResponse = await _client.SendAsync(first);
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        using var second = CreateAuthorizedJsonRequest(HttpMethod.Post, "/api/admin/notification-channel-configs", RbacRoles.SystemAdministrator, "admin-1", body);
        var secondResponse = await _client.SendAsync(second);
        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
    }

    /// <summary>
    /// Kiểm tra tạo cấu hình không hợp lệ trả về BadRequest.
    /// </summary>
    [Fact]
    public async Task CreateNotificationChannelConfig_WithInvalidRequest_ReturnsBadRequest()
    {
        using var request = CreateAuthorizedJsonRequest(HttpMethod.Post, "/api/admin/notification-channel-configs", RbacRoles.SystemAdministrator, "admin-1", new
        {
            channel = "SMS",
            displayName = "",
            isEnabled = true
        });

        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
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

    private sealed record NotificationChannelConfigResponse(
        Guid Id,
        string Channel,
        string DisplayName,
        string? Description,
        bool IsEnabled,
        DateTime CreatedAtUtc,
        DateTime UpdatedAtUtc);
}
