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

public sealed class CreateNotificationE2ETests : IAsyncLifetime
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
    public async Task CreateNotification_AuthenticatedIntegrationClient_CreatesAndReturnsSameIdOnRetry()
    {
        using var firstRequest = CreateAuthorizedRequest("idem-e2e", "integration-client-e2e");
        using var secondRequest = CreateAuthorizedRequest("idem-e2e", "integration-client-e2e");

        var firstResponse = await _client.SendAsync(firstRequest);
        var secondResponse = await _client.SendAsync(secondRequest);

        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);

        var firstPayload = await firstResponse.Content.ReadFromJsonAsync<CreateNotificationResponsePayload>();
        var secondPayload = await secondResponse.Content.ReadFromJsonAsync<CreateNotificationResponsePayload>();

        Assert.NotNull(firstPayload);
        Assert.NotNull(secondPayload);
        Assert.NotEqual(Guid.Empty, firstPayload!.NotificationId);
        Assert.Equal(firstPayload.NotificationId, secondPayload!.NotificationId);
    }

    private static HttpRequestMessage CreateAuthorizedRequest(string idempotencyKey, string subject)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/notifications")
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
}