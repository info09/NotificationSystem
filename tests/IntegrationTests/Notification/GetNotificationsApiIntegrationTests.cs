using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using IdentityAccess.Domain;
using IntegrationMessaging.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Notification.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace IntegrationTests.Notification;

public sealed class GetNotificationsApiIntegrationTests : IAsyncLifetime
{
    private const string Issuer = "test-issuer";
    private const string Audience = "test-audience";
    private const string SigningKey = "integration-test-signing-key-min-32-chars-0001";

    private readonly PostgreSqlContainer _postgreSqlContainer = TestcontainersConfiguration.CreatePostgreSqlBuilder().Build();
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
        });

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
            await dbContext.Database.MigrateAsync();
            var integrationDbContext = scope.ServiceProvider.GetRequiredService<IntegrationMessagingDbContext>();
            await integrationDbContext.Database.MigrateAsync();
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
    public async Task GetNotifications_WithoutToken_ReturnsUnauthorizedProblemDetails()
    {
        var response = await _client.GetAsync("/api/notifications");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal((int)HttpStatusCode.Unauthorized, problem!.Status);
    }

    [Fact]
    public async Task GetNotificationById_WithoutToken_ReturnsUnauthorizedProblemDetails()
    {
        var response = await _client.GetAsync($"/api/notifications/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task GetNotifications_WithIntegrationClientScope_ReturnsOnlyOwnNotifications()
    {
        var firstId = await CreateNotificationAsync("integration-client-1", "idem-a", "email", "Pending");
        var secondId = await CreateNotificationAsync("integration-client-2", "idem-b", "inapp", "Pending");
        Assert.NotEqual(Guid.Empty, firstId);
        Assert.NotEqual(Guid.Empty, secondId);

        using var request = CreateAuthorizedGetRequest("/api/notifications", RbacRoles.IntegrationClient, "integration-client-1");
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<GetNotificationsResponsePayload>();
        Assert.NotNull(payload);
        Assert.Equal(1, payload!.TotalCount);
        Assert.Single(payload.Items);
        Assert.Equal(firstId, payload.Items[0].Id);
        Assert.Equal("integration-client-1", await GetIdempotencyScopeAsync(firstId));
    }

    [Fact]
    public async Task GetNotifications_WithBusinessOperator_ReturnsAllNotifications()
    {
        var firstId = await CreateNotificationAsync("integration-client-1", "idem-op-a", "email", "Pending");
        var secondId = await CreateNotificationAsync("integration-client-2", "idem-op-b", "inapp", "Pending");

        using var request = CreateAuthorizedGetRequest("/api/notifications?page=1&pageSize=10", RbacRoles.BusinessOperator, "operator-1");
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<GetNotificationsResponsePayload>();
        Assert.NotNull(payload);
        Assert.True(payload!.TotalCount >= 2);
        Assert.Contains(payload.Items, item => item.Id == firstId);
        Assert.Contains(payload.Items, item => item.Id == secondId);
        Assert.Equal(1, payload.Page);
        Assert.Equal(10, payload.PageSize);
    }

    [Fact]
    public async Task GetNotifications_WithFilters_ReturnsMatchingNotifications()
    {
        await CreateNotificationAsync("integration-client-1", "idem-filter-1", "email", "Pending");
        var inAppId = await CreateNotificationAsync("integration-client-1", "idem-filter-2", "inapp", "Pending");

        using var request = CreateAuthorizedGetRequest(
            "/api/notifications?status=Pending&channel=inapp",
            RbacRoles.SystemAdministrator,
            "admin-1");
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<GetNotificationsResponsePayload>();
        Assert.NotNull(payload);
        Assert.All(payload!.Items, item =>
        {
            Assert.Equal("Pending", item.Status);
            Assert.Equal("InApp", item.Channel);
        });
        Assert.Contains(payload.Items, item => item.Id == inAppId);
    }

    [Fact]
    public async Task GetNotifications_WithCreatedTimeFilter_ReturnsMatchingNotifications()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var notificationId = await CreateNotificationAsync("integration-client-1", "idem-time-1", "email", "Pending");
        var after = DateTime.UtcNow.AddSeconds(1);

        using var request = CreateAuthorizedGetRequest(
            $"/api/notifications?createdFrom={Uri.EscapeDataString(before.ToString("O"))}&createdTo={Uri.EscapeDataString(after.ToString("O"))}",
            RbacRoles.SystemAdministrator,
            "admin-1");
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<GetNotificationsResponsePayload>();
        Assert.NotNull(payload);
        Assert.Contains(payload!.Items, item => item.Id == notificationId);
    }

    [Fact]
    public async Task GetNotifications_WithInvalidQuery_ReturnsBadRequestValidationProblem()
    {
        using var request = CreateAuthorizedGetRequest(
            "/api/notifications?status=Unknown&channel=sms&page=0&pageSize=101&createdFrom=not-a-date",
            RbacRoles.SystemAdministrator,
            "admin-1");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal((int)HttpStatusCode.BadRequest, problem!.Status);
    }

    [Fact]
    public async Task GetNotificationById_WithBusinessOperator_ReturnsNotification()
    {
        var notificationId = await CreateNotificationAsync("integration-client-1", "idem-detail-1", "email", "Pending");

        using var request = CreateAuthorizedGetRequest($"/api/notifications/{notificationId}", RbacRoles.BusinessOperator, "operator-1");
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<NotificationResponsePayload>();
        Assert.NotNull(payload);
        Assert.Equal(notificationId, payload!.Id);
        Assert.Equal("Pending", payload.Status);
        Assert.Equal("Email", payload.Channel);
        Assert.NotEqual(default, payload.CreatedAt);
    }

    [Fact]
    public async Task GetNotificationById_WithIntegrationClientAccessingOtherScope_ReturnsNotFound()
    {
        var notificationId = await CreateNotificationAsync("integration-client-1", "idem-detail-scope", "email", "Pending");

        using var request = CreateAuthorizedGetRequest($"/api/notifications/{notificationId}", RbacRoles.IntegrationClient, "integration-client-2");
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetNotificationById_WhenNotFound_ReturnsNotFound()
    {
        using var request = CreateAuthorizedGetRequest($"/api/notifications/{Guid.NewGuid()}", RbacRoles.SystemAdministrator, "admin-1");
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<Guid> CreateNotificationAsync(string subject, string idempotencyKey, string channel, string expectedStatus)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/notifications")
        {
            Content = JsonContent.Create(new
            {
                idempotencyKey,
                recipient = "user@example.com",
                channel,
                content = "Xin chao",
                notificationTemplateId = (Guid?)null
            })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue(
            JwtBearerDefaults.AuthenticationScheme,
            CreateToken(RbacRoles.IntegrationClient, subject));

        var response = await _client.SendAsync(request);
        Assert.True(
            response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.OK,
            $"Unexpected status code: {response.StatusCode}");

        var payload = await response.Content.ReadFromJsonAsync<CreateNotificationResponsePayload>();
        Assert.NotNull(payload);
        Assert.Equal(expectedStatus, payload!.Status);
        return payload.NotificationId;
    }

    private async Task<string?> GetIdempotencyScopeAsync(Guid notificationId)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
        var notification = await dbContext.Notifications.AsNoTracking().FirstOrDefaultAsync(n => n.Id == notificationId);
        return notification?.IdempotencyScope;
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