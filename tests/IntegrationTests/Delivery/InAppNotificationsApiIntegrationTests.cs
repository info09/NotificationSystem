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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Notification.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace IntegrationTests.Delivery;

public sealed class InAppNotificationsApiIntegrationTests : IAsyncLifetime
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
    public async Task GetInAppNotifications_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/in-app-notifications");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
    }

    [Fact]
    public async Task GetInAppNotifications_ReturnsOnlyCurrentUserItems()
    {
        var own = await SeedAsync("user-1", Guid.NewGuid(), "Mine", "Content");
        await SeedAsync("user-2", Guid.NewGuid(), "Other", "Content");

        using var request = CreateAuthorizedGetRequest("/api/in-app-notifications?page=1&pageSize=10", "user-1");
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<GetInAppNotificationsResponsePayload>();
        Assert.NotNull(payload);
        Assert.Single(payload!.Items);
        Assert.Equal(own, payload.Items[0].Id);
        Assert.Equal("Unread", payload.Items[0].ReadStatus);
    }

    [Fact]
    public async Task GetUnreadCount_ReturnsCurrentUserUnreadCount()
    {
        await SeedAsync("user-1", Guid.NewGuid(), "A", "A");
        await SeedAsync("user-1", Guid.NewGuid(), "B", "B");
        await SeedAsync("user-2", Guid.NewGuid(), "C", "C");

        using var request = CreateAuthorizedGetRequest("/api/in-app-notifications/unread-count", "user-1");
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<UnreadCountResponsePayload>();
        Assert.NotNull(payload);
        Assert.Equal(2, payload!.UnreadCount);
    }

    [Fact]
    public async Task GetById_WhenBelongsToOtherUser_ReturnsNotFound()
    {
        var id = await SeedAsync("user-1", Guid.NewGuid(), "Mine", "Content");

        using var request = CreateAuthorizedGetRequest($"/api/in-app-notifications/{id}", "user-2");
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
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
        var request = new HttpRequestMessage(HttpMethod.Get, url);
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
