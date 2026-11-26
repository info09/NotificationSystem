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
/// Integration test cho API quản trị Định tuyến sự kiện tích hợp.
/// </summary>
public sealed class IntegrationEventRouteApiIntegrationTests : IAsyncLifetime
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
            await scope.ServiceProvider.GetRequiredService<Administration.Infrastructure.Persistence.AdministrationDbContext>().Database.MigrateAsync();
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
    /// Kiểm tra vòng đời CRUD của Định tuyến sự kiện tích hợp.
    /// </summary>
    [Fact]
    public async Task IntegrationEventRouteLifecycle_WithSystemAdministrator_Works()
    {
        var createBody = new
        {
            eventType = "OrderCreated",
            notificationTemplateId = (Guid?)null,
            channel = "Email",
            description = "Định tuyến order created",
            isActive = true
        };

        using var createRequest = CreateAuthorizedJsonRequest(HttpMethod.Post, "/api/admin/integration-event-routes", RbacRoles.SystemAdministrator, "admin-1", createBody);
        var createResponse = await _client.SendAsync(createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<IntegrationEventRouteResponse>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(created);

        using var updateRequest = CreateAuthorizedJsonRequest(HttpMethod.Put, $"/api/admin/integration-event-routes/{created!.Id}", RbacRoles.SystemAdministrator, "admin-1", new
        {
            eventType = "OrderCreated",
            notificationTemplateId = (Guid?)null,
            channel = "InApp",
            description = "Cập nhật",
            isActive = false
        });
        var updateResponse = await _client.SendAsync(updateRequest);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        using var deleteRequest = CreateAuthorizedRequest(HttpMethod.Delete, $"/api/admin/integration-event-routes/{created.Id}", RbacRoles.SystemAdministrator, "admin-1");
        var deleteResponse = await _client.SendAsync(deleteRequest);
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var administrationDbContext = scope.ServiceProvider.GetRequiredService<Administration.Infrastructure.Persistence.AdministrationDbContext>();
        var auditActions = await administrationDbContext.AuditLogRecords
            .Where(x => x.TargetType == "IntegrationEventRoute" && x.TargetId == created.Id.ToString())
            .Select(x => x.Action)
            .ToListAsync();
        Assert.Contains("CreateIntegrationEventRoute", auditActions);
        Assert.Contains("UpdateIntegrationEventRoute", auditActions);
        Assert.Contains("DeleteIntegrationEventRoute", auditActions);
    }

    /// <summary>
    /// Kiểm tra thiếu mapping trả về ValidationError.
    /// </summary>
    [Fact]
    public async Task CreateIntegrationEventRoute_WithoutMapping_ReturnsBadRequest()
    {
        using var request = CreateAuthorizedJsonRequest(HttpMethod.Post, "/api/admin/integration-event-routes", RbacRoles.SystemAdministrator, "admin-1", new
        {
            eventType = "OrderCreated",
            notificationTemplateId = (Guid?)null,
            channel = (string?)null,
            isActive = true
        });

        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>
    /// Kiểm tra EventType trùng trả về Conflict.
    /// </summary>
    [Fact]
    public async Task CreateIntegrationEventRoute_WithDuplicateEventType_ReturnsConflict()
    {
        var body = new
        {
            eventType = "OrderCreated",
            notificationTemplateId = (Guid?)null,
            channel = "Email",
            isActive = true
        };

        using var first = CreateAuthorizedJsonRequest(HttpMethod.Post, "/api/admin/integration-event-routes", RbacRoles.SystemAdministrator, "admin-1", body);
        var firstResponse = await _client.SendAsync(first);
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        using var second = CreateAuthorizedJsonRequest(HttpMethod.Post, "/api/admin/integration-event-routes", RbacRoles.SystemAdministrator, "admin-1", body);
        var secondResponse = await _client.SendAsync(second);
        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
    }

    /// <summary>
    /// Kiểm tra truy cập không có quyền trả về Forbidden.
    /// </summary>
    [Fact]
    public async Task GetIntegrationEventRoutes_WithBusinessOperator_ReturnsForbidden()
    {
        using var request = CreateAuthorizedRequest(HttpMethod.Get, "/api/admin/integration-event-routes", RbacRoles.BusinessOperator, "operator-1");
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

    private sealed record IntegrationEventRouteResponse(
        Guid Id,
        string EventType,
        Guid? NotificationTemplateId,
        string? Channel,
        string? Description,
        bool IsActive,
        DateTime CreatedAtUtc,
        DateTime UpdatedAtUtc);
}
