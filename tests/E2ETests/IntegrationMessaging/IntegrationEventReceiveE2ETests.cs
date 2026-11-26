using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Delivery.Application.Abstractions;
using Delivery.Infrastructure.Persistence;
using IdentityAccess.Domain;
using IdentityAccess.Infrastructure.Persistence;
using IntegrationMessaging.Infrastructure.Messaging;
using IntegrationMessaging.Infrastructure.Persistence;
using IntegrationTests;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Notification.Application.Notifications;
using Notification.Infrastructure;
using Notification.Infrastructure.Persistence;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using TemplateConfiguration.Domain.IntegrationEventRoutes;
using TemplateConfiguration.Infrastructure.Persistence;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using Worker;
using Worker.Outbox;
using DomainNotification = global::Notification.Domain.Notification;

namespace E2ETests.IntegrationMessaging;

/// <summary>
/// E2E test cho luồng tiếp nhận Sự kiện tích hợp.
/// </summary>
public sealed class IntegrationEventReceiveE2ETests : IAsyncLifetime
{
    private const string Issuer = "e2e-issuer";
    private const string Audience = "e2e-audience";
    private const string SigningKey = "e2e-test-signing-key-min-32-chars-000001";

    private readonly PostgreSqlContainer _postgreSqlContainer = TestcontainersConfiguration.CreatePostgreSqlBuilder().Build();
    private readonly RabbitMqContainer _rabbitMqContainer = new RabbitMqBuilder()
        .WithImage("rabbitmq:3-management")
        .WithUsername("guest")
        .WithPassword("guest")
        .Build();
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    /// <summary>
    /// Khởi tạo môi trường test.
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
            builder.UseSetting("RabbitMq:UserName", "guest");
            builder.UseSetting("RabbitMq:Password", "guest");
            builder.UseSetting("RabbitMq:VirtualHost", "/");
        });

        using (var scope = _factory.Services.CreateScope())
        {
            await scope.ServiceProvider.GetRequiredService<NotificationDbContext>().Database.MigrateAsync();
            await scope.ServiceProvider.GetRequiredService<IntegrationMessagingDbContext>().Database.MigrateAsync();
            await scope.ServiceProvider.GetRequiredService<DeliveryDbContext>().Database.MigrateAsync();
            await scope.ServiceProvider.GetRequiredService<IdentityAccessDbContext>().Database.MigrateAsync();
            await scope.ServiceProvider.GetRequiredService<TemplateConfigurationDbContext>().Database.MigrateAsync();
            await scope.ServiceProvider.GetRequiredService<Administration.Infrastructure.Persistence.AdministrationDbContext>().Database.MigrateAsync();

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
        await Task.WhenAll(_postgreSqlContainer.DisposeAsync().AsTask(), _rabbitMqContainer.DisposeAsync().AsTask());
    }

    /// <summary>
    /// Kiểm tra tiếp nhận Sự kiện tích hợp end-to-end qua Outbox/RabbitMQ/Worker và chống trùng.
    /// </summary>
    [Fact]
    public async Task ReceiveIntegrationEvent_EndToEnd_CreatesNotificationThroughOutboxAndWorker()
    {
        var rabbitMqOptions = CreateRabbitMqOptions();
        await new RabbitMqTopologyInitializer(rabbitMqOptions).InitializeAsync();

        var body = new
        {
            messageId = Guid.NewGuid(),
            eventId = Guid.NewGuid(),
            eventType = "OrderCreated",
            occurredAt = DateTime.UtcNow,
            source = "order-service",
            idempotencyKey = "idem-e2e-1",
            version = "1.0",
            payload = new { orderId = 123, customerName = "Nguyen Van A", recipient = "user@example.com", content = "Đơn hàng đã tạo" }
        };

        using var firstRequest = CreateAuthorizedJsonRequest(HttpMethod.Post, "/api/integration-events", RbacRoles.IntegrationClient, "integration-1", body);
        var firstResponse = await _client.SendAsync(firstRequest);
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        using var secondRequest = CreateAuthorizedJsonRequest(HttpMethod.Post, "/api/integration-events", RbacRoles.IntegrationClient, "integration-1", body);
        var secondResponse = await _client.SendAsync(secondRequest);
        Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);

        var duplicate = await secondResponse.Content.ReadFromJsonAsync<ReceiveIntegrationEventResponse>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(duplicate);
        Assert.True(duplicate!.Duplicate);

        var publisher = new OutboxPublisherBackgroundService(
            new SingleServiceScopeFactory(_factory.Services),
            new RabbitMqMessagePublisher(rabbitMqOptions),
            NullLogger<OutboxPublisherBackgroundService>.Instance);

        var published = await publisher.PublishPendingMessagesAsync(CancellationToken.None);
        Assert.True(published >= 1);

        var (connection, channel, ea) = await GetDeliveryEventArgsAsync(rabbitMqOptions.Value, RabbitMqTopologyNames.EmailQueue);
        await using var _ = connection;
        await using var __ = channel;

        var worker = CreateWorker(rabbitMqOptions);
        await worker.HandleMessageAsync(channel, ea, CancellationToken.None);

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IntegrationMessagingDbContext>();
        Assert.Equal(1, await dbContext.IntegrationEvents.CountAsync(x => x.MessageId == body.messageId));

        var notificationDbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
        var notification = await notificationDbContext.Notifications.SingleAsync(x => x.Recipient.Value == "user@example.com");
        Assert.Equal(global::Notification.Domain.NotificationStatus.Sent, notification.Status);
    }

    private IOptions<RabbitMqOptions> CreateRabbitMqOptions() =>
        Options.Create(new RabbitMqOptions
        {
            Host = _rabbitMqContainer.Hostname,
            Port = _rabbitMqContainer.GetMappedPublicPort(5672),
            UserName = "guest",
            Password = "guest",
            VirtualHost = "/"
        });

    private NotificationWorker CreateWorker(IOptions<RabbitMqOptions> options)
    {
        var services = new ServiceCollection();
        services.AddDbContext<IntegrationMessagingDbContext>(o => o.UseNpgsql(_postgreSqlContainer.GetConnectionString()));
        services.AddDbContext<NotificationDbContext>(o => o.UseNpgsql(_postgreSqlContainer.GetConnectionString()));
        services.AddDbContext<DeliveryDbContext>(o => o.UseNpgsql(_postgreSqlContainer.GetConnectionString()));
        services.AddScoped<INotificationOutboxWriter, NotificationOutboxWriter>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<BuildingBlocks.SharedKernel.IntegrationEvents.IIntegrationNotificationCreator, IntegrationNotificationCreator>();
        services.AddScoped<INotificationChannelDispatcher, NoOpDispatcher>();
        services.AddScoped<IDeliveryLogRecorder, NoOpDeliveryLogRecorder>();
        var provider = services.BuildServiceProvider();

        return new NotificationWorker(
            new SingleServiceScopeFactory(provider),
            options,
            NullLogger<NotificationWorker>.Instance);
    }

    private static async Task<(IConnection Connection, IChannel Channel, BasicDeliverEventArgs Ea)> GetDeliveryEventArgsAsync(RabbitMqOptions options, string queueName)
    {
        var factory = new ConnectionFactory
        {
            HostName = options.Host,
            Port = options.Port,
            UserName = options.UserName,
            Password = options.Password,
            VirtualHost = options.VirtualHost
        };

        var connection = await factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();
        var consumer = new AsyncEventingBasicConsumer(channel);
        var tcs = new TaskCompletionSource<BasicDeliverEventArgs>(TaskCreationOptions.RunContinuationsAsynchronously);
        consumer.ReceivedAsync += (_, ea) =>
        {
            tcs.TrySetResult(ea);
            return Task.CompletedTask;
        };
        await channel.BasicConsumeAsync(queueName, autoAck: false, consumer: consumer);
        var ea = await tcs.Task.WaitAsync(TimeSpan.FromSeconds(15));
        return (connection, channel, ea);
    }

    private static HttpRequestMessage CreateAuthorizedJsonRequest(HttpMethod method, string url, string role, string subject, object body)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", CreateToken(role, subject));
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

    private sealed record ReceiveIntegrationEventResponse(
        Guid Id,
        Guid MessageId,
        Guid EventId,
        string EventType,
        DateTime ReceivedAtUtc,
        bool Duplicate,
        Guid? NotificationId);

    private sealed class SingleServiceScopeFactory : IServiceScopeFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public SingleServiceScopeFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IServiceScope CreateScope() => _serviceProvider.CreateScope();
    }

    private sealed class NoOpDispatcher : INotificationChannelDispatcher
    {
        public Task DispatchAsync(NotificationChannelDispatchRequest request, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class NoOpDeliveryLogRecorder : IDeliveryLogRecorder
    {
        public Task RecordAttemptAsync(RecordDeliveryAttemptRequest request, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
