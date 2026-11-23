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
/// Integration test cho API Hàng đợi thư chết.
/// </summary>
public sealed class DeadLetterQueueApiIntegrationTests : IAsyncLifetime
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
    public async Task GetDeadLetterMessages_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/admin/dead-letter-queues/notifications.email.dlq/messages");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetDeadLetterMessages_WithBusinessOperator_ReturnsMessages()
    {
        _gateway.Messages =
        [
            new DeadLetterQueueMessageDto("notifications.email.dlq", "msg-1", Guid.NewGuid().ToString(), "Email", "notification.email.created", "NotificationCreated", "rejected", DateTime.UtcNow, 1, "{}")
        ];

        using var request = CreateAuthorizedRequest(HttpMethod.Get, "/api/admin/dead-letter-queues/notifications.email.dlq/messages?limit=10", RbacRoles.BusinessOperator, "operator-1");
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<GetDeadLetterQueueMessagesResponse>();
        Assert.NotNull(payload);
        Assert.Single(payload!.Items);
    }

    [Fact]
    public async Task ResendDeadLetterMessage_WithSystemAdministrator_ReturnsResentAndWritesAudit()
    {
        var notificationId = await SeedNotificationAsync();
        _gateway.ResendResult = new DeadLetterResendGatewayResult("msg-1", Guid.NewGuid(), notificationId, "notifications.email.dlq");

        using var request = CreateAuthorizedRequest(HttpMethod.Post, "/api/admin/dead-letter-queues/notifications.email.dlq/messages/msg-1/resend", RbacRoles.SystemAdministrator, "admin-1");
        request.Content = JsonContent.Create(new { idempotencyKey = "idem-1" });
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<ResendDeadLetterMessageResponse>();
        Assert.NotNull(payload);
        Assert.Equal("Resent", payload!.Status);

        using var scope = _factory.Services.CreateScope();
        var administrationDbContext = scope.ServiceProvider.GetRequiredService<AdministrationDbContext>();
        var audit = await administrationDbContext.AuditLogRecords.SingleAsync();
        Assert.Equal("ResendDeadLetterMessage", audit.Action);
        Assert.Equal("Success", audit.Result);
    }

    /// <summary>
    /// Tạo yêu cầu HTTP có token xác thực.
    /// </summary>
    /// <param name="method">HTTP method.</param>
    /// <param name="url">URL yêu cầu.</param>
    /// <param name="role">Vai trò người dùng.</param>
    /// <param name="subject">Định danh người dùng.</param>
    /// <returns>Yêu cầu HTTP đã gắn token.</returns>
    private static HttpRequestMessage CreateAuthorizedRequest(HttpMethod method, string url, string role, string subject)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", CreateToken(role, subject));
        return request;
    }

    /// <summary>
    /// Tạo JWT token phục vụ test.
    /// </summary>
    /// <param name="role">Vai trò người dùng.</param>
    /// <param name="subject">Định danh người dùng.</param>
    /// <returns>Chuỗi JWT token.</returns>
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

    /// <summary>
    /// Tạo dữ liệu Thông báo phục vụ test resend.
    /// </summary>
    /// <returns>Mã định danh Thông báo.</returns>
    private async Task<Guid> SeedNotificationAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
        var notification = global::Notification.Domain.Notification.Create(
            Guid.NewGuid(),
            new global::Notification.Domain.Recipient("user@example.com"),
            global::Notification.Domain.NotificationChannel.Email,
            "hello",
            null,
            "test-scope",
            Guid.NewGuid().ToString());
        notification.MarkProcessing();
        notification.MarkFailed();
        dbContext.Notifications.Add(notification);
        await dbContext.SaveChangesAsync();
        return notification.Id;
    }

    /// <summary>
    /// Gateway giả lập Hàng đợi thư chết cho integration test.
    /// </summary>
    private sealed class FakeDeadLetterQueueGateway : IDeadLetterQueueGateway
    {
        /// <summary>
        /// Danh sách Thông điệp giả lập.
        /// </summary>
        public IReadOnlyList<DeadLetterQueueMessageDto> Messages { get; set; } = [];

        /// <summary>
        /// Kết quả gửi lại giả lập.
        /// </summary>
        public DeadLetterResendGatewayResult? ResendResult { get; set; }

        /// <summary>
        /// Lấy danh sách Thông điệp giả lập.
        /// </summary>
        public Task<IReadOnlyList<DeadLetterQueueMessageDto>> GetMessagesAsync(string queueName, int limit, CancellationToken cancellationToken)
            => Task.FromResult(Messages);

        /// <summary>
        /// Gửi lại Thông điệp giả lập.
        /// </summary>
        public Task<DeadLetterResendGatewayResult?> ResendAsync(string queueName, string messageId, CancellationToken cancellationToken)
            => Task.FromResult(ResendResult);
    }
}
