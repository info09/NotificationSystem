using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Delivery.Domain.InAppNotifications;
using Delivery.Infrastructure.Persistence;
using IdentityAccess.Domain;
using IntegrationMessaging.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Notification.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace E2ETests.Delivery;

public sealed class InAppNotificationsE2ETests : IAsyncLifetime
{
    private const string Issuer = "e2e-issuer";
    private const string Audience = "e2e-audience";
    private const string SigningKey = "e2e-test-signing-key-min-32-chars-000001";

    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16")
        .WithDatabase("notification_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

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
            builder.UseSetting("OpenTelemetry:Enabled", "true");
            builder.UseSetting("OpenTelemetry:Tracing:Enabled", "true");
            builder.UseSetting("OpenTelemetry:Metrics:Enabled", "true");
            builder.UseSetting("OpenTelemetry:Exporter:Console:Enabled", "false");
            builder.ConfigureTestServices(services =>
            {
                services.PostConfigure<HealthCheckServiceOptions>(options =>
                {
                    options.Registrations.Clear();
                    options.Registrations.Add(new HealthCheckRegistration(
                        "ready-stub",
                        new AlwaysHealthyHealthCheck(),
                        HealthStatus.Unhealthy,
                        tags: ["ready"]));
                });
            });
        });

        using (var scope = _factory.Services.CreateScope())
        {
            await scope.ServiceProvider.GetRequiredService<NotificationDbContext>().Database.MigrateAsync();
            await scope.ServiceProvider.GetRequiredService<IntegrationMessagingDbContext>().Database.MigrateAsync();
            await scope.ServiceProvider.GetRequiredService<DeliveryDbContext>().Database.MigrateAsync();
        }

        _client = _factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
        await _postgreSqlContainer.DisposeAsync();
    }

    [Fact]
    public async Task GetInAppNotifications_AndUnreadCount_ReturnsOwnData()
    {
        var ownId = await SeedAsync("user-1", Guid.NewGuid(), "Mine", "Content");
        await SeedAsync("user-2", Guid.NewGuid(), "Other", "Content");

        using var listRequest = CreateAuthorizedGetRequest("/api/in-app-notifications?page=1&pageSize=10", "user-1");
        var listResponse = await _client.SendAsync(listRequest);

        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        var listPayload = await listResponse.Content.ReadFromJsonAsync<GetInAppNotificationsResponsePayload>();
        Assert.NotNull(listPayload);
        Assert.Single(listPayload!.Items);
        Assert.Equal(ownId, listPayload.Items[0].Id);

        using var countRequest = CreateAuthorizedGetRequest("/api/in-app-notifications/unread-count", "user-1");
        var countResponse = await _client.SendAsync(countRequest);
        Assert.Equal(HttpStatusCode.OK, countResponse.StatusCode);
        var countPayload = await countResponse.Content.ReadFromJsonAsync<UnreadCountResponsePayload>();
        Assert.NotNull(countPayload);
        Assert.Equal(1, countPayload!.UnreadCount);
    }

    [Fact]
    public async Task MarkAsRead_WhenOwned_UpdatesReadStatus()
    {
        var id = await SeedAsync("user-1", Guid.NewGuid(), "Mine", "Content");

        using var markRequest = CreateAuthorizedRequest(HttpMethod.Put, $"/api/in-app-notifications/{id}/read", "user-1");
        var markResponse = await _client.SendAsync(markRequest);

        Assert.Equal(HttpStatusCode.OK, markResponse.StatusCode);
        var marked = await markResponse.Content.ReadFromJsonAsync<InAppNotificationResponsePayload>();
        Assert.NotNull(marked);
        Assert.Equal("Read", marked!.ReadStatus);
    }

    private async Task<Guid> SeedAsync(string recipientUserId, Guid notificationId, string title, string content)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DeliveryDbContext>();
        var entity = InAppNotification.Create(Guid.NewGuid(), notificationId, recipientUserId, title, content);
        dbContext.InAppNotifications.Add(entity);
        await dbContext.SaveChangesAsync();
        return entity.Id;
    }

    private static HttpRequestMessage CreateAuthorizedGetRequest(string url, string subject)
    {
        return CreateAuthorizedRequest(HttpMethod.Get, url, subject);
    }

    private static HttpRequestMessage CreateAuthorizedRequest(HttpMethod method, string url, string subject)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue(
            JwtBearerDefaults.AuthenticationScheme,
            CreateToken(subject));
        return request;
    }

    private static string CreateToken(string subject)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims:
            [
                new Claim(ClaimTypes.NameIdentifier, subject),
                new Claim(ClaimTypes.Name, subject),
                new Claim(ClaimTypes.Role, RbacRoles.BusinessOperator)
            ],
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private sealed class AlwaysHealthyHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default) =>
            Task.FromResult(HealthCheckResult.Healthy());
    }

    private sealed record InAppNotificationResponsePayload(
        Guid Id,
        Guid NotificationId,
        string RecipientUserId,
        string? Title,
        string? Content,
        string ReadStatus,
        DateTime CreatedAtUtc,
        DateTime? ReadAtUtc);

    private sealed record GetInAppNotificationsResponsePayload(
        IReadOnlyList<InAppNotificationResponsePayload> Items,
        int TotalCount,
        int Page,
        int PageSize);

    private sealed record UnreadCountResponsePayload(int UnreadCount);
}
