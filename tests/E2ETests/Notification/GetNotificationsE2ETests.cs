using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using IdentityAccess.Domain;
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

namespace E2ETests.Notification;

public sealed class GetNotificationsE2ETests : IAsyncLifetime
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
            var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
            await dbContext.Database.EnsureCreatedAsync();
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
    public async Task GetNotificationsAndDetail_AsBusinessOperator_ReturnsCreatedNotification()
    {
        var notificationId = await CreateNotificationAsync("integration-client-e2e", "idem-e2e-query");

        using var listRequest = CreateAuthorizedGetRequest("/api/notifications?status=Pending&channel=email", RbacRoles.BusinessOperator, "operator-e2e");
        var listResponse = await _client.SendAsync(listRequest);

        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        var listPayload = await listResponse.Content.ReadFromJsonAsync<GetNotificationsResponsePayload>();
        Assert.NotNull(listPayload);
        Assert.Contains(listPayload!.Items, item => item.Id == notificationId);

        using var detailRequest = CreateAuthorizedGetRequest($"/api/notifications/{notificationId}", RbacRoles.BusinessOperator, "operator-e2e");
        var detailResponse = await _client.SendAsync(detailRequest);

        Assert.Equal(HttpStatusCode.OK, detailResponse.StatusCode);
        var detailPayload = await detailResponse.Content.ReadFromJsonAsync<NotificationResponsePayload>();
        Assert.NotNull(detailPayload);
        Assert.Equal(notificationId, detailPayload!.Id);
        Assert.Equal("Pending", detailPayload.Status);
    }

    [Fact]
    public async Task GetNotifications_AsIntegrationClient_ReturnsOnlyOwnScopedNotifications()
    {
        var ownId = await CreateNotificationAsync("integration-client-e2e-1", "idem-e2e-own");
        var otherId = await CreateNotificationAsync("integration-client-e2e-2", "idem-e2e-other");

        using var request = CreateAuthorizedGetRequest("/api/notifications", RbacRoles.IntegrationClient, "integration-client-e2e-1");
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<GetNotificationsResponsePayload>();
        Assert.NotNull(payload);
        Assert.Contains(payload!.Items, item => item.Id == ownId);
        Assert.DoesNotContain(payload.Items, item => item.Id == otherId);
    }

    private async Task<Guid> CreateNotificationAsync(string subject, string idempotencyKey)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/notifications")
        {
            Content = JsonContent.Create(new
            {
                idempotencyKey,
                recipient = "user@example.com",
                channel = "email",
                content = "Xin chao",
                notificationTemplateId = (Guid?)null
            })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue(
            JwtBearerDefaults.AuthenticationScheme,
            CreateToken(RbacRoles.IntegrationClient, subject));

        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<CreateNotificationResponsePayload>();
        Assert.NotNull(payload);
        return payload!.NotificationId;
    }

    private static HttpRequestMessage CreateAuthorizedGetRequest(string url, string role, string subject)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue(
            JwtBearerDefaults.AuthenticationScheme,
            CreateToken(role, subject));
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
                new Claim(ClaimTypes.Name, subject),
                new Claim(ClaimTypes.Role, role)
            ],
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private sealed class AlwaysHealthyHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(HealthCheckResult.Healthy());
    }

    private sealed record CreateNotificationResponsePayload(Guid NotificationId, string Status);

    private sealed record NotificationResponsePayload(
        Guid Id,
        string Recipient,
        string Channel,
        string Status,
        string? Content,
        Guid? NotificationTemplateId,
        DateTime CreatedAt);

    private sealed record GetNotificationsResponsePayload(
        IReadOnlyList<NotificationResponsePayload> Items,
        int TotalCount,
        int Page,
        int PageSize);
}