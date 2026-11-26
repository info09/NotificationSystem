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
using TemplateConfiguration.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace IntegrationTests.TemplateConfiguration;

/// <summary>
/// Integration test cho API quản trị Mẫu thông báo.
/// </summary>
public sealed class NotificationTemplateApiIntegrationTests : IAsyncLifetime
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
    /// Kiểm tra vòng đời CRUD của Mẫu thông báo với quyền Quản trị viên hệ thống.
    /// </summary>
    [Fact]
    public async Task NotificationTemplateLifecycle_WithSystemAdministrator_Works()
    {
        var createBody = new
        {
            templateKey = "order-created",
            name = "Đơn hàng mới",
            description = "Mẫu email đơn hàng",
            channel = "Email",
            subject = "Đơn hàng {{orderId}}",
            body = "Xin chào {{name}}",
            isActive = true
        };

        using var createRequest = CreateAuthorizedJsonRequest(HttpMethod.Post, "/api/admin/notification-templates", RbacRoles.SystemAdministrator, "admin-1", createBody);
        var createResponse = await _client.SendAsync(createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<NotificationTemplateResponse>();
        Assert.NotNull(created);
        Assert.Equal("order-created", created!.TemplateKey);

        using var getRequest = CreateAuthorizedRequest(HttpMethod.Get, $"/api/admin/notification-templates/{created.Id}", RbacRoles.SystemAdministrator, "admin-1");
        var getResponse = await _client.SendAsync(getRequest);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var updateBody = new
        {
            name = "Đơn hàng mới cập nhật",
            description = "Mô tả mới",
            channel = "Email",
            subject = "Tiêu đề mới",
            body = "Nội dung mới",
            isActive = false
        };
        using var updateRequest = CreateAuthorizedJsonRequest(HttpMethod.Put, $"/api/admin/notification-templates/{created.Id}", RbacRoles.SystemAdministrator, "admin-1", updateBody);
        var updateResponse = await _client.SendAsync(updateRequest);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await updateResponse.Content.ReadFromJsonAsync<NotificationTemplateResponse>();
        Assert.Equal("Đơn hàng mới cập nhật", updated!.Name);
        Assert.False(updated.IsActive);

        using var deleteRequest = CreateAuthorizedRequest(HttpMethod.Delete, $"/api/admin/notification-templates/{created.Id}", RbacRoles.SystemAdministrator, "admin-1");
        var deleteResponse = await _client.SendAsync(deleteRequest);
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

        using var getAfterDeleteRequest = CreateAuthorizedRequest(HttpMethod.Get, $"/api/admin/notification-templates/{created.Id}", RbacRoles.SystemAdministrator, "admin-1");
        var getAfterDeleteResponse = await _client.SendAsync(getAfterDeleteRequest);
        Assert.Equal(HttpStatusCode.NotFound, getAfterDeleteResponse.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var administrationDbContext = scope.ServiceProvider.GetRequiredService<Administration.Infrastructure.Persistence.AdministrationDbContext>();
        var auditActions = await administrationDbContext.AuditLogRecords
            .Where(x => x.TargetType == "NotificationTemplate" && x.TargetId == created.Id.ToString())
            .Select(x => x.Action)
            .ToListAsync();
        Assert.Contains("CreateNotificationTemplate", auditActions);
        Assert.Contains("UpdateNotificationTemplate", auditActions);
        Assert.Contains("DeleteNotificationTemplate", auditActions);
    }

    /// <summary>
    /// Kiểm tra tạo trùng TemplateKey trả về Conflict.
    /// </summary>
    [Fact]
    public async Task CreateNotificationTemplate_WithDuplicateTemplateKey_ReturnsConflict()
    {
        var body = new { templateKey = "dup-key", name = "Tên", description = (string?)null, channel = "Email", subject = (string?)null, body = "Body", isActive = true };

        using var first = CreateAuthorizedJsonRequest(HttpMethod.Post, "/api/admin/notification-templates", RbacRoles.SystemAdministrator, "admin-1", body);
        var firstResponse = await _client.SendAsync(first);
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        using var second = CreateAuthorizedJsonRequest(HttpMethod.Post, "/api/admin/notification-templates", RbacRoles.SystemAdministrator, "admin-1", body);
        var secondResponse = await _client.SendAsync(second);
        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
    }

    /// <summary>
    /// Kiểm tra truy cập không có quyền trả về Forbidden.
    /// </summary>
    [Fact]
    public async Task GetNotificationTemplates_WithBusinessOperator_ReturnsForbidden()
    {
        using var request = CreateAuthorizedRequest(HttpMethod.Get, "/api/admin/notification-templates", RbacRoles.BusinessOperator, "operator-1");
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    /// <summary>
    /// Kiểm tra tạo mẫu thiếu dữ liệu bắt buộc trả về BadRequest.
    /// </summary>
    [Fact]
    public async Task CreateNotificationTemplate_WithInvalidRequest_ReturnsBadRequest()
    {
        using var request = CreateAuthorizedJsonRequest(HttpMethod.Post, "/api/admin/notification-templates", RbacRoles.SystemAdministrator, "admin-1", new
        {
            templateKey = "",
            name = "",
            channel = "SMS",
            body = "",
            isActive = true
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

    private sealed record NotificationTemplateResponse(
        Guid Id,
        string TemplateKey,
        string Name,
        string? Description,
        string Channel,
        string? Subject,
        string Body,
        bool IsActive,
        DateTime CreatedAtUtc,
        DateTime UpdatedAtUtc);
}
