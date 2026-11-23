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

public sealed class CreateNotificationApiIntegrationTests : IAsyncLifetime
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
    public async Task CreateNotification_WithoutToken_ReturnsUnauthorizedProblemDetails()
    {
        var response = await _client.PostAsJsonAsync("/api/notifications", CreateValidRequest("idem-unauth"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal((int)HttpStatusCode.Unauthorized, problem!.Status);
    }

    [Fact]
    public async Task CreateNotification_WithWrongRole_ReturnsForbiddenProblemDetails()
    {
        using var request = CreateAuthorizedRequest(CreateValidRequest("idem-forbidden"), RbacRoles.BusinessOperator, "business-operator-1");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal((int)HttpStatusCode.Forbidden, problem!.Status);
    }

    [Fact]
    public async Task CreateNotification_WithInvalidRequest_ReturnsBadRequestValidationProblem()
    {
        var invalidRequest = new
        {
            idempotencyKey = "",
            recipient = "",
            channel = "sms",
            content = (string?)null,
            notificationTemplateId = (Guid?)null
        };

        using var request = CreateAuthorizedRequest(invalidRequest, RbacRoles.IntegrationClient, "integration-client-1");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal((int)HttpStatusCode.BadRequest, problem!.Status);
    }

    [Fact]
    public async Task CreateNotification_WithValidRequest_ReturnsCreatedWithNotificationId()
    {
        using var request = CreateAuthorizedRequest(CreateValidRequest("idem-created"), RbacRoles.IntegrationClient, "integration-client-1");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<CreateNotificationResponsePayload>();
        Assert.NotNull(payload);
        Assert.NotEqual(Guid.Empty, payload!.NotificationId);
        Assert.Equal("Pending", payload.Status);
    }

    [Fact]
    public async Task CreateNotification_WithSameIdempotencyKeyAndScope_ReturnsSameNotificationId()
    {
        using var firstRequest = CreateAuthorizedRequest(CreateValidRequest("idem-duplicate"), RbacRoles.IntegrationClient, "integration-client-1");
        using var secondRequest = CreateAuthorizedRequest(CreateValidRequest("idem-duplicate"), RbacRoles.IntegrationClient, "integration-client-1");

        var firstResponse = await _client.SendAsync(firstRequest);
        var secondResponse = await _client.SendAsync(secondRequest);

        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);

        var firstPayload = await firstResponse.Content.ReadFromJsonAsync<CreateNotificationResponsePayload>();
        var secondPayload = await secondResponse.Content.ReadFromJsonAsync<CreateNotificationResponsePayload>();

        Assert.NotNull(firstPayload);
        Assert.NotNull(secondPayload);
        Assert.Equal(firstPayload!.NotificationId, secondPayload!.NotificationId);
        Assert.Equal("Pending", secondPayload.Status);
    }

    [Fact]
    public async Task CreateNotification_WithSameIdempotencyKeyButDifferentScope_CreatesDistinctNotifications()
    {
        using var firstRequest = CreateAuthorizedRequest(CreateValidRequest("idem-shared-key"), RbacRoles.IntegrationClient, "integration-client-1");
        using var secondRequest = CreateAuthorizedRequest(CreateValidRequest("idem-shared-key"), RbacRoles.IntegrationClient, "integration-client-2");

        var firstResponse = await _client.SendAsync(firstRequest);
        var secondResponse = await _client.SendAsync(secondRequest);

        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Created, secondResponse.StatusCode);

        var firstPayload = await firstResponse.Content.ReadFromJsonAsync<CreateNotificationResponsePayload>();
        var secondPayload = await secondResponse.Content.ReadFromJsonAsync<CreateNotificationResponsePayload>();

        Assert.NotNull(firstPayload);
        Assert.NotNull(secondPayload);
        Assert.NotEqual(firstPayload!.NotificationId, secondPayload!.NotificationId);
    }

    [Fact]
    public async Task CreateNotification_WithValidRequest_PersistsOutboxMessageInSameBusinessFlow()
    {
        using var request = CreateAuthorizedRequest(CreateValidRequest("idem-outbox"), RbacRoles.IntegrationClient, "integration-client-1");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<CreateNotificationResponsePayload>();
        Assert.NotNull(payload);

        using var scope = _factory.Services.CreateScope();
        var outboxDbContext = scope.ServiceProvider.GetRequiredService<IntegrationMessagingDbContext>();
        var outboxMessages = await outboxDbContext.OutboxMessages.AsNoTracking().ToListAsync();

        Assert.Single(outboxMessages);
        Assert.Equal("NotificationCreated", outboxMessages[0].EventType);
        Assert.Equal("Pending", outboxMessages[0].Status.ToString());
        Assert.Equal(0, outboxMessages[0].RetryCount);
        Assert.True(outboxMessages[0].NextAttemptAtUtc <= DateTime.UtcNow.AddMinutes(1));
        Assert.Null(outboxMessages[0].LastError);
        Assert.Contains(payload!.NotificationId.ToString(), outboxMessages[0].Payload);
    }

    [Fact]
    public async Task CreateNotification_WithDuplicateIdempotencyKey_DoesNotCreateAdditionalOutboxMessage()
    {
        using var firstRequest = CreateAuthorizedRequest(CreateValidRequest("idem-outbox-dup"), RbacRoles.IntegrationClient, "integration-client-1");
        using var secondRequest = CreateAuthorizedRequest(CreateValidRequest("idem-outbox-dup"), RbacRoles.IntegrationClient, "integration-client-1");

        var firstResponse = await _client.SendAsync(firstRequest);
        var secondResponse = await _client.SendAsync(secondRequest);

        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var outboxDbContext = scope.ServiceProvider.GetRequiredService<IntegrationMessagingDbContext>();
        var outboxMessages = await outboxDbContext.OutboxMessages.AsNoTracking().ToListAsync();

        Assert.Single(outboxMessages);
    }

    private static object CreateValidRequest(string idempotencyKey) => new
    {
        idempotencyKey,
        recipient = "user@example.com",
        channel = "email",
        content = "Xin chao",
        notificationTemplateId = (Guid?)null
    };

    private static HttpRequestMessage CreateAuthorizedRequest(object payload, string role, string subject)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/notifications")
        {
            Content = JsonContent.Create(payload)
        };

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
}