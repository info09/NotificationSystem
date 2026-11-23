using Administration.Application.DeadLetterQueue;
using Administration.Infrastructure.Persistence;
using IdentityAccess.Domain;
using IntegrationMessaging.Infrastructure.Messaging;
using IntegrationMessaging.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Notification.Infrastructure.Persistence;
using RabbitMQ.Client;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace E2ETests.Administration;

/// <summary>
/// E2E test cho luồng truy vấn và Gửi lại Hàng đợi thư chết.
/// </summary>
public sealed class DeadLetterQueueResendE2ETests : IAsyncLifetime
{
    private const string Issuer = "test-issuer";
    private const string Audience = "test-audience";
    private const string SigningKey = "e2e-test-signing-key-min-32-chars-000001";

    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    private readonly RabbitMqContainer _rabbitMqContainer = new RabbitMqBuilder()
        .WithImage("rabbitmq:3-management")
        .WithPortBinding(15672, true)
        .Build();

    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    /// <summary>
    /// Khởi tạo môi trường E2E.
    /// </summary>
    public async Task InitializeAsync()
    {
        await Task.WhenAll(_postgreSqlContainer.StartAsync(), _rabbitMqContainer.StartAsync());

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", _postgreSqlContainer.GetConnectionString());
            builder.UseSetting("JWT_ISSUER", Issuer);
            builder.UseSetting("JWT_AUDIENCE", Audience);
            builder.UseSetting("JWT_SIGNING_KEY", SigningKey);
            builder.UseSetting("RabbitMq:Host", _rabbitMqContainer.Hostname);
            builder.UseSetting("RabbitMq:Port", _rabbitMqContainer.GetMappedPublicPort(5672).ToString());
            builder.UseSetting("RabbitMq:ManagementPort", _rabbitMqContainer.GetMappedPublicPort(15672).ToString());
            builder.UseSetting("RabbitMq:UserName", "rabbitmq");
            builder.UseSetting("RabbitMq:Password", "rabbitmq");
        });

        using (var scope = _factory.Services.CreateScope())
        {
            await scope.ServiceProvider.GetRequiredService<NotificationDbContext>().Database.MigrateAsync();
            await scope.ServiceProvider.GetRequiredService<IntegrationMessagingDbContext>().Database.MigrateAsync();
            await scope.ServiceProvider.GetRequiredService<AdministrationDbContext>().Database.MigrateAsync();
        }

        var options = Options.Create(new RabbitMqOptions
        {
            Host = _rabbitMqContainer.Hostname,
            Port = _rabbitMqContainer.GetMappedPublicPort(5672),
            UserName = "rabbitmq",
            Password = "rabbitmq"
        });
        await new RabbitMqTopologyInitializer(options).InitializeAsync();
        _client = _factory.CreateClient();
    }

    /// <summary>
    /// Dọn dẹp môi trường E2E.
    /// </summary>
    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
        await Task.WhenAll(_postgreSqlContainer.DisposeAsync().AsTask(), _rabbitMqContainer.DisposeAsync().AsTask());
    }

    [Fact]
    public async Task ResendDeadLetterMessage_WithRealDlqMessage_RepublishesAndWritesAudit()
    {
        var notificationId = await SeedNotificationAsync();
        var messageId = Guid.NewGuid().ToString();
        await PublishDlqMessageAsync(messageId, notificationId);

        using var getRequest = CreateAuthorizedRequest(HttpMethod.Get, "/api/admin/dead-letter-queues/notifications.email.dlq/messages?limit=10", RbacRoles.BusinessOperator, "operator-1");
        var getResponse = await _client.SendAsync(getRequest);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var list = await getResponse.Content.ReadFromJsonAsync<GetDeadLetterQueueMessagesResponse>();
        Assert.NotNull(list);
        Assert.Contains(list!.Items, x => x.MessageId == messageId);

        using var resendRequest = CreateAuthorizedRequest(HttpMethod.Post, $"/api/admin/dead-letter-queues/notifications.email.dlq/messages/{messageId}/resend", RbacRoles.SystemAdministrator, "admin-1");
        resendRequest.Content = JsonContent.Create(new { idempotencyKey = "idem-e2e-1" });
        var resendResponse = await _client.SendAsync(resendRequest);

        Assert.Equal(HttpStatusCode.OK, resendResponse.StatusCode);
        var payload = await resendResponse.Content.ReadFromJsonAsync<ResendDeadLetterMessageResponse>();
        Assert.NotNull(payload);
        Assert.Equal("Resent", payload!.Status);

        using var scope = _factory.Services.CreateScope();
        var administrationDbContext = scope.ServiceProvider.GetRequiredService<AdministrationDbContext>();
        var audit = await administrationDbContext.AuditLogRecords.SingleAsync(x => x.Action == "ResendDeadLetterMessage");
        Assert.Equal("Success", audit.Result);

        var resentId = payload.ResentMessageId.ToString();
        var published = await TryGetMessageByIdAsync(RabbitMqTopologyNames.EmailQueue, resentId);
        Assert.NotNull(published);
    }

    [Fact]
    public async Task ResendDeadLetterMessage_WithSameIdempotencyKey_ReturnsDuplicate()
    {
        var notificationId = await SeedNotificationAsync();
        var messageId = Guid.NewGuid().ToString();
        await PublishDlqMessageAsync(messageId, notificationId);

        using var firstRequest = CreateAuthorizedRequest(HttpMethod.Post, $"/api/admin/dead-letter-queues/notifications.email.dlq/messages/{messageId}/resend", RbacRoles.SystemAdministrator, "admin-1");
        firstRequest.Content = JsonContent.Create(new { idempotencyKey = "idem-e2e-dup-1" });
        var firstResponse = await _client.SendAsync(firstRequest);
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);

        using var secondRequest = CreateAuthorizedRequest(HttpMethod.Post, $"/api/admin/dead-letter-queues/notifications.email.dlq/messages/{messageId}/resend", RbacRoles.SystemAdministrator, "admin-1");
        secondRequest.Content = JsonContent.Create(new { idempotencyKey = "idem-e2e-dup-1" });
        var secondResponse = await _client.SendAsync(secondRequest);

        Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);
        var payload = await secondResponse.Content.ReadFromJsonAsync<ResendDeadLetterMessageResponse>();
        Assert.NotNull(payload);
        Assert.Equal("Duplicate", payload!.Status);
    }

    /// <summary>
    /// Tạo dữ liệu Thông báo phục vụ E2E.
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
    /// Phát hành Thông điệp trực tiếp vào Hàng đợi thư chết.
    /// </summary>
    /// <param name="messageId">Mã định danh Thông điệp.</param>
    /// <param name="notificationId">Mã định danh Thông báo.</param>
    private async Task PublishDlqMessageAsync(string messageId, Guid notificationId)
    {
        var factory = new ConnectionFactory
        {
            HostName = _rabbitMqContainer.Hostname,
            Port = _rabbitMqContainer.GetMappedPublicPort(5672),
            UserName = "rabbitmq",
            Password = "rabbitmq"
        };

        await using var connection = await factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new
        {
            notificationId,
            channel = "Email",
            eventType = "NotificationCreated"
        }));

        var props = new BasicProperties
        {
            Persistent = true,
            MessageId = messageId,
            ContentType = "application/json",
            Type = "NotificationCreated",
            Headers = new Dictionary<string, object?>
            {
                ["x-death"] = new List<object>
                {
                    new Dictionary<string, object>
                    {
                        ["reason"] = "rejected",
                        ["count"] = 1L,
                        ["queue"] = RabbitMqTopologyNames.EmailQueue,
                        ["exchange"] = RabbitMqTopologyNames.NotificationsExchange,
                        ["routing-keys"] = new List<object> { Encoding.UTF8.GetBytes(RabbitMqTopologyNames.EmailRoutingKeyPattern) },
                        ["time"] = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                    }
                }
            }
        };

        await channel.BasicPublishAsync(string.Empty, RabbitMqTopologyNames.EmailDeadLetterQueue, false, props, body);
    }

    /// <summary>
    /// Thử lấy Thông điệp theo MessageId từ queue.
    /// </summary>
    /// <param name="queueName">Tên queue.</param>
    /// <param name="messageId">Mã định danh Thông điệp.</param>
    /// <returns>Thông điệp nếu tìm thấy; ngược lại null.</returns>
    private async Task<BasicGetResult?> TryGetMessageByIdAsync(string queueName, string messageId)
    {
        var factory = new ConnectionFactory
        {
            HostName = _rabbitMqContainer.Hostname,
            Port = _rabbitMqContainer.GetMappedPublicPort(5672),
            UserName = "rabbitmq",
            Password = "rabbitmq"
        };

        await using var connection = await factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();
        for (var i = 0; i < 20; i++)
        {
            var result = await channel.BasicGetAsync(queueName, autoAck: true);
            if (result is null)
            {
                await Task.Delay(200);
                continue;
            }

            if (string.Equals(result.BasicProperties?.MessageId, messageId, StringComparison.OrdinalIgnoreCase))
            {
                return result;
            }
        }

        return null;
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
}
