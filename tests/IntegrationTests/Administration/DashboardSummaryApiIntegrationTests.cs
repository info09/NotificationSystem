using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Administration.Application.DeadLetterQueue;
using Administration.Infrastructure.Persistence;
using IdentityAccess.Domain;
using IntegrationMessaging.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using Notification.Infrastructure.Persistence;
using System.IdentityModel.Tokens.Jwt;
using Testcontainers.PostgreSql;

namespace IntegrationTests.Administration;

/// <summary>
/// Integration test cho API tổng hợp Tổng quan.
/// </summary>
public sealed class DashboardSummaryApiIntegrationTests : IAsyncLifetime
{
    private const string Issuer = "test-issuer";
    private const string Audience = "test-audience";
    private const string SigningKey = "integration-test-signing-key-min-32-chars-0001";

    private readonly PostgreSqlContainer _postgreSqlContainer = TestcontainersConfiguration.CreatePostgreSqlBuilder().Build();
    private readonly FakeDeadLetterQueueGateway _gateway = new();
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
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IDeadLetterQueueGateway>();
                services.AddSingleton<IDeadLetterQueueGateway>(_gateway);
            });
        });

        using (var scope = _factory.Services.CreateScope())
        {
            var notificationDbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
            await notificationDbContext.Database.MigrateAsync();
            var integrationDbContext = scope.ServiceProvider.GetRequiredService<IntegrationMessagingDbContext>();
            await integrationDbContext.Database.MigrateAsync();
            var administrationDbContext = scope.ServiceProvider.GetRequiredService<AdministrationDbContext>();
            await administrationDbContext.Database.MigrateAsync();
        }

        _client = _factory.CreateClient();
    }

    /// <summary>
    /// Dọn dẹp môi trường test.
    /// </summary>
    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
        await _postgreSqlContainer.DisposeAsync();
    }

    [Fact]
    public async Task GetDashboardSummary_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/admin/dashboard/summary");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetDashboardSummary_WithBusinessOperator_ReturnsSummary()
    {
        _gateway.MessageCounts["notifications.email.dlq"] = 3;
        _gateway.MessageCounts["notifications.inapp.dlq"] = 1;

        using var request = CreateAuthorizedRequest(HttpMethod.Get, "/api/admin/dashboard/summary", RbacRoles.BusinessOperator, "operator-1");
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<DashboardSummaryResponse>();
        Assert.NotNull(payload);
        Assert.NotNull(payload!.Notifications);
        Assert.Equal(2, payload.DeadLetterQueues.Count);
        Assert.Contains(payload.DeadLetterQueues, x => x.QueueName == "notifications.email.dlq" && x.Count == 3);
        Assert.False(string.IsNullOrWhiteSpace(payload.Health.Status));
    }

    private static HttpRequestMessage CreateAuthorizedRequest(HttpMethod method, string url, string role, string subject)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", CreateToken(role, subject));
        return request;
    }

    private static string CreateToken(string role, string subject)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, subject),
            new Claim(ClaimTypes.NameIdentifier, subject),
            new Claim(ClaimTypes.Role, role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private sealed record DashboardSummaryResponse(
        NotificationStatusSummaryDto Notifications,
        IReadOnlyList<DeadLetterQueueSummaryDto> DeadLetterQueues,
        HealthSummaryDto Health);

    private sealed record NotificationStatusSummaryDto(int Pending, int Processing, int Sent, int Failed, string? Error);

    private sealed record DeadLetterQueueSummaryDto(string QueueName, int Count, string? Error);

    private sealed record HealthSummaryDto(string Status, string? Details);

    private sealed class FakeDeadLetterQueueGateway : IDeadLetterQueueGateway
    {
        public Dictionary<string, int> MessageCounts { get; } = new(StringComparer.OrdinalIgnoreCase);

        public Task<IReadOnlyList<DeadLetterQueueMessageDto>> GetMessagesAsync(string queueName, int limit, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<DeadLetterQueueMessageDto>>([]);

        public Task<int> GetMessageCountAsync(string queueName, CancellationToken cancellationToken)
            => Task.FromResult(MessageCounts.TryGetValue(queueName, out var count) ? count : 0);

        public Task<DeadLetterResendGatewayResult?> ResendAsync(string queueName, string messageId, CancellationToken cancellationToken)
            => Task.FromResult<DeadLetterResendGatewayResult?>(null);
    }
}
