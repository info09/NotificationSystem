using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Administration.Infrastructure.Persistence;
using IdentityAccess.Domain;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Testcontainers.PostgreSql;

namespace IntegrationTests.Administration;

/// <summary>
/// Integration test cho API truy vấn Nhật ký kiểm toán.
/// </summary>
public sealed class AuditLogApiIntegrationTests : IAsyncLifetime
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
            await scope.ServiceProvider.GetRequiredService<AdministrationDbContext>().Database.MigrateAsync();
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
    /// Kiểm tra truy vấn Nhật ký kiểm toán theo quyền hợp lệ trả về danh sách.
    /// </summary>
    [Fact]
    public async Task GetAuditLogs_WithSystemAdministrator_ReturnsOk()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AdministrationDbContext>();
        dbContext.AuditLogRecords.Add(global::Administration.Domain.AuditLogRecord.Create(
            "admin-1", RbacRoles.SystemAdministrator, "CreateNotificationTemplate", "NotificationTemplate", Guid.NewGuid().ToString(), "Success"));
        await dbContext.SaveChangesAsync();

        using var request = CreateAuthorizedRequest(HttpMethod.Get, "/api/admin/audit-logs?page=1&pageSize=20", RbacRoles.SystemAdministrator, "admin-1");
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<GetAuditLogsResponse>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(payload);
        Assert.True(payload!.TotalCount >= 1);
    }

    /// <summary>
    /// Kiểm tra vai trò không có quyền truy vấn trả về Forbidden.
    /// </summary>
    [Fact]
    public async Task GetAuditLogs_WithIntegrationClient_ReturnsForbidden()
    {
        using var request = CreateAuthorizedRequest(HttpMethod.Get, "/api/admin/audit-logs", RbacRoles.IntegrationClient, "integration-1");
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private static HttpRequestMessage CreateAuthorizedRequest(HttpMethod method, string url, string role, string subject)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", CreateToken(role, subject));
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

    private sealed record GetAuditLogsResponse(IReadOnlyList<AuditLogItem> Items, int TotalCount, int Page, int PageSize);

    private sealed record AuditLogItem(Guid Id, string Action, string TargetType, string TargetId, string Result);
}
