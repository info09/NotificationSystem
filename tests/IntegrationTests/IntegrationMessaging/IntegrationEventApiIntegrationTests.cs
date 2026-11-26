using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
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
using TemplateConfiguration.Domain.IntegrationEventRoutes;
using TemplateConfiguration.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace IntegrationTests.IntegrationMessaging;

/// <summary>
/// Integration test cho API tiếp nhận Sự kiện tích hợp.
/// </summary>
public sealed class IntegrationEventApiIntegrationTests : IAsyncLifetime
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

            var templateConfigurationDbContext = scope.ServiceProvider.GetRequiredService<TemplateConfigurationDbContext>();
            templateConfigurationDbContext.IntegrationEventRoutes.Add(
                IntegrationEventRoute.Create(Guid.NewGuid(), "OrderCreated", null, "Email", "Route test", true));
            await templateConfigurationDbContext.SaveChangesAsync();
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
    /// Kiểm tra tiếp nhận Sự kiện tích hợp hợp lệ trả về Created, ghi nhận truy vết và tạo Thông báo.
    /// </summary>
    [Fact]
    public async Task ReceiveIntegrationEvent_WithValidRequest_ReturnsCreatedPersistsTraceAndCreatesNotification()
    {
        var requestBody = CreateValidRequest(Guid.NewGuid());

        using var request = CreateAuthorizedJsonRequest(HttpMethod.Post, "/api/integration-events", RbacRoles.IntegrationClient, "integration-1", requestBody);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<ReceiveIntegrationEventResponse>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(payload);
        Assert.False(payload!.Duplicate);
        Assert.Equal(requestBody.MessageId, payload.MessageId);
        Assert.NotEqual(Guid.Empty, payload.NotificationId);

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IntegrationMessagingDbContext>();
        var record = await dbContext.IntegrationEvents.SingleAsync(x => x.MessageId == requestBody.MessageId);
        Assert.Equal(requestBody.EventType, record.EventType);
        Assert.Equal(requestBody.Source, record.Source);

        var notificationDbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
        var notification = await notificationDbContext.Notifications.SingleAsync(x => x.Id == payload.NotificationId);
        Assert.Equal("user@example.com", notification.Recipient.Value);
    }

    /// <summary>
    /// Kiểm tra tiếp nhận trùng MessageId trả về Ok với Duplicate.
    /// </summary>
    [Fact]
    public async Task ReceiveIntegrationEvent_WithDuplicateMessageId_ReturnsOkDuplicate()
    {
        var requestBody = CreateValidRequest(Guid.NewGuid());

        using var first = CreateAuthorizedJsonRequest(HttpMethod.Post, "/api/integration-events", RbacRoles.IntegrationClient, "integration-1", requestBody);
        var firstResponse = await _client.SendAsync(first);
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        using var second = CreateAuthorizedJsonRequest(HttpMethod.Post, "/api/integration-events", RbacRoles.IntegrationClient, "integration-1", requestBody);
        var secondResponse = await _client.SendAsync(second);
        Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);

        var payload = await secondResponse.Content.ReadFromJsonAsync<ReceiveIntegrationEventResponse>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(payload);
        Assert.True(payload!.Duplicate);
    }

    /// <summary>
    /// Kiểm tra envelope thiếu trường bắt buộc trả về BadRequest.
    /// </summary>
    [Fact]
    public async Task ReceiveIntegrationEvent_WithInvalidEnvelope_ReturnsBadRequest()
    {
        using var request = CreateAuthorizedJsonRequest(HttpMethod.Post, "/api/integration-events", RbacRoles.IntegrationClient, "integration-1", new
        {
            messageId = (Guid?)null,
            eventId = (Guid?)null,
            eventType = "",
            occurredAt = (DateTime?)null,
            source = "",
            idempotencyKey = "",
            version = "",
            payload = (object?)null
        });

        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>
    /// Kiểm tra vai trò không có quyền trả về Forbidden.
    /// </summary>
    [Fact]
    public async Task ReceiveIntegrationEvent_WithBusinessOperator_ReturnsForbidden()
    {
        var requestBody = CreateValidRequest(Guid.NewGuid());

        using var request = CreateAuthorizedJsonRequest(HttpMethod.Post, "/api/integration-events", RbacRoles.BusinessOperator, "operator-1", requestBody);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private static ReceiveIntegrationEventRequestBody CreateValidRequest(Guid messageId) => new(
        messageId,
        Guid.NewGuid(),
        "OrderCreated",
        DateTime.UtcNow,
        "order-service",
        "idem-1",
        "1.0",
        new { orderId = 123, recipient = "user@example.com", content = "Đơn hàng đã tạo" });

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

    private sealed record ReceiveIntegrationEventRequestBody(
        Guid MessageId,
        Guid EventId,
        string EventType,
        DateTime OccurredAt,
        string Source,
        string? IdempotencyKey,
        string Version,
        object Payload);

    private sealed record ReceiveIntegrationEventResponse(
        Guid Id,
        Guid MessageId,
        Guid EventId,
        string EventType,
        DateTime ReceivedAtUtc,
        bool Duplicate,
        Guid? NotificationId);
}
