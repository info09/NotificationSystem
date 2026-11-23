using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Delivery.Domain.DeliveryLogs;
using Delivery.Infrastructure.Persistence;
using IdentityAccess.Domain;
using IntegrationMessaging.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Notification.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace IntegrationTests.Delivery;

/// <summary>
/// Integration test cho API truy vấn lịch sử gửi Thông báo.
/// </summary>
public sealed class NotificationDeliveryHistoryApiIntegrationTests : IAsyncLifetime
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
            builder.UseSetting("Delivery:RetentionDays", "90");
        });

        using (var scope = _factory.Services.CreateScope())
        {
            await scope.ServiceProvider.GetRequiredService<NotificationDbContext>().Database.MigrateAsync();
            await scope.ServiceProvider.GetRequiredService<IntegrationMessagingDbContext>().Database.MigrateAsync();
            await scope.ServiceProvider.GetRequiredService<DeliveryDbContext>().Database.MigrateAsync();
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
    public async Task GetDeliveryHistory_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync($"/api/notifications/{Guid.NewGuid()}/delivery-history");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetDeliveryHistory_WithReadRole_ReturnsLogsAndAttempts()
    {
        var notificationId = Guid.NewGuid();
        await SeedAsync(notificationId, "Email", DeliveryAttemptResult.Failed, "err", 1, 1, DateTime.UtcNow.AddMinutes(-5));
        await SeedAsync(notificationId, "Email", DeliveryAttemptResult.Success, null, 2, 1, DateTime.UtcNow);

        using var request = CreateAuthorizedRequest(HttpMethod.Get, $"/api/notifications/{notificationId}/delivery-history?page=1&pageSize=10", RbacRoles.BusinessOperator);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<DeliveryHistoryResponsePayload>();
        Assert.NotNull(payload);
        Assert.Single(payload!.Logs);
        Assert.Equal(2, payload.Attempts.TotalCount);
        Assert.Equal("Email", payload.Logs[0].Channel);
    }

    private async Task SeedAsync(Guid notificationId, string channel, DeliveryAttemptResult result, string? error, int attemptNumber, int retryCount, DateTime atUtc)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DeliveryDbContext>();

        dbContext.DeliveryAttempts.Add(DeliveryAttempt.Create(Guid.NewGuid(), notificationId, channel, attemptNumber, result, error, "corr-1", atUtc));
        var log = await dbContext.NotificationDeliveryLogs.FirstOrDefaultAsync(x => x.NotificationId == notificationId && x.Channel == channel);
        if (log is null)
        {
            dbContext.NotificationDeliveryLogs.Add(NotificationDeliveryLog.Create(Guid.NewGuid(), notificationId, channel, result, error, retryCount, "corr-1", atUtc));
        }
        else
        {
            log.Update(result, error, retryCount, "corr-1");
        }

        await dbContext.SaveChangesAsync();
    }

    private static HttpRequestMessage CreateAuthorizedRequest(HttpMethod method, string url, string role)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", CreateToken(role));
        return request;
    }

    private static string CreateToken(string role)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, role)
        };

        var token = new JwtSecurityToken(Issuer, Audience, claims, expires: DateTime.UtcNow.AddMinutes(30), signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private sealed class DeliveryHistoryResponsePayload
    {
        public List<DeliveryLogPayload> Logs { get; set; } = [];
        public DeliveryAttemptsPayload Attempts { get; set; } = new();
    }

    private sealed class DeliveryLogPayload
    {
        public Guid NotificationId { get; set; }
        public string Channel { get; set; } = string.Empty;
        public string Result { get; set; } = string.Empty;
        public int RetryCount { get; set; }
    }

    private sealed class DeliveryAttemptsPayload
    {
        public List<DeliveryAttemptPayload> Items { get; set; } = [];
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    private sealed class DeliveryAttemptPayload
    {
        public Guid NotificationId { get; set; }
        public string Channel { get; set; } = string.Empty;
        public int AttemptNumber { get; set; }
        public string Result { get; set; } = string.Empty;
    }
}
