using System.Text.Json;
using IntegrationMessaging.Infrastructure.Messaging;
using IntegrationMessaging.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Notification.Application.Notifications;
using Notification.Infrastructure.Persistence;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using Worker;
using DomainNotification = global::Notification.Domain.Notification;
using NotificationChannel = global::Notification.Domain.NotificationChannel;
using NotificationStatus = global::Notification.Domain.NotificationStatus;

namespace E2ETests.IntegrationMessaging;

/// <summary>
/// E2E test cho NotificationWorker consume và idempotency.
/// </summary>
public sealed class NotificationWorkerE2ETests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16")
        .WithDatabase("notification_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private readonly RabbitMqContainer _rabbitMqContainer = new RabbitMqBuilder()
        .WithImage("rabbitmq:3-management")
        .WithUsername("guest")
        .WithPassword("guest")
        .Build();

    /// <summary>
    /// Khởi động các container phụ thuộc.
    /// </summary>
    public async Task InitializeAsync()
    {
        await Task.WhenAll(_postgreSqlContainer.StartAsync(), _rabbitMqContainer.StartAsync());
    }

    /// <summary>
    /// Dọn dẹp các container phụ thuộc.
    /// </summary>
    public async Task DisposeAsync()
    {
        await Task.WhenAll(_postgreSqlContainer.DisposeAsync().AsTask(), _rabbitMqContainer.DisposeAsync().AsTask());
    }

    [Fact]
    public async Task HandleMessageAsync_WithNewMessage_CreatesInboxAndAcks()
    {
        var options = CreateRabbitMqOptions();
        await new RabbitMqTopologyInitializer(options).InitializeAsync();
        await using (var dbContext = CreateDbContext())
        {
            await dbContext.Database.EnsureCreatedAsync();
        }

        var messageId = Guid.NewGuid();
        await PublishTestMessageAsync(options.Value, RabbitMqTopologyNames.InAppQueue, "notification.inapp.created", messageId, Guid.NewGuid());

        var (consumeConnection, consumeChannel, ea) = await GetDeliveryEventArgsAsync(options.Value, RabbitMqTopologyNames.InAppQueue);
        await using var _ = consumeConnection;
        await using var __ = consumeChannel;

        var worker = CreateWorker(options);
        await worker.HandleMessageAsync(consumeChannel, ea, CancellationToken.None);

        await using var verifyContext = CreateDbContext();
        var inbox = await verifyContext.InboxMessages.SingleAsync();
        Assert.Equal(messageId, inbox.MessageId);
        Assert.Equal("NotificationWorker", inbox.ConsumerName);
    }

    [Fact]
    public async Task HandleMessageAsync_WithPendingNotification_UpdatesStatusToSent()
    {
        var options = CreateRabbitMqOptions();
        await new RabbitMqTopologyInitializer(options).InitializeAsync();
        await using (var integrationDbContext = CreateDbContext())
        {
            await integrationDbContext.Database.MigrateAsync();
        }
        await using (var notificationDbContext = CreateNotificationDbContext())
        {
            await notificationDbContext.Database.MigrateAsync();
        }

        var notification = DomainNotification.Create(
            Guid.NewGuid(),
            new global::Notification.Domain.Recipient("user@example.com"),
            NotificationChannel.Email,
            "Xin chao",
            null,
            "scope-1",
            "key-1",
            DateTime.UtcNow);
        await using (var notificationDbContext = CreateNotificationDbContext())
        {
            notificationDbContext.Notifications.Add(notification);
            await notificationDbContext.SaveChangesAsync();
        }

        var messageId = Guid.NewGuid();
        await PublishTestMessageAsync(options.Value, RabbitMqTopologyNames.EmailQueue, "notification.email.created", messageId, notification.Id);

        var (consumeConnection, consumeChannel, ea) = await GetDeliveryEventArgsAsync(options.Value, RabbitMqTopologyNames.EmailQueue);
        await using var _ = consumeConnection;
        await using var __ = consumeChannel;

        var worker = CreateWorker(options, useRealNotificationRepository: true);
        await worker.HandleMessageAsync(consumeChannel, ea, CancellationToken.None);

        await using var verifyNotificationDbContext = CreateNotificationDbContext();
        var persisted = await verifyNotificationDbContext.Notifications.AsNoTracking().SingleAsync(x => x.Id == notification.Id);
        Assert.Equal(NotificationStatus.Sent, persisted.Status);
    }

    private IntegrationMessagingDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<IntegrationMessagingDbContext>()
            .UseNpgsql(
                _postgreSqlContainer.GetConnectionString(),
                npgsqlOptions => npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "integration_messaging"))
            .Options;

        return new IntegrationMessagingDbContext(options);
    }

    private static async Task<(IConnection connection, IChannel channel, BasicDeliverEventArgs ea)> GetDeliveryEventArgsAsync(RabbitMqOptions options, string queue)
    {
        var factory = CreateConnectionFactory(options);
        var connection = await factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();

        var result = await channel.BasicGetAsync(queue, autoAck: false);
        Assert.NotNull(result);

        var ea = new BasicDeliverEventArgs(
            consumerTag: "test-consumer",
            deliveryTag: result!.DeliveryTag,
            redelivered: result.Redelivered,
            exchange: result.Exchange,
            routingKey: result.RoutingKey,
            properties: result.BasicProperties,
            body: result.Body);

        return (connection, channel, ea);
    }

    private static ConnectionFactory CreateConnectionFactory(RabbitMqOptions options) =>
        new()
        {
            HostName = options.Host,
            Port = options.Port,
            UserName = options.UserName,
            Password = options.Password,
            VirtualHost = options.VirtualHost
        };

    private IOptions<RabbitMqOptions> CreateRabbitMqOptions() =>
        Options.Create(new RabbitMqOptions
        {
            Host = _rabbitMqContainer.Hostname,
            Port = _rabbitMqContainer.GetMappedPublicPort(5672),
            UserName = "guest",
            Password = "guest",
            VirtualHost = "/"
        });

    private NotificationWorker CreateWorker(IOptions<RabbitMqOptions> options, bool useRealNotificationRepository = false)
    {
        var services = new ServiceCollection();
        services.AddDbContext<IntegrationMessagingDbContext>(dbOptions =>
            dbOptions.UseNpgsql(
                _postgreSqlContainer.GetConnectionString(),
                npgsqlOptions => npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "integration_messaging")));

        if (useRealNotificationRepository)
        {
            services.AddDbContext<NotificationDbContext>(dbOptions =>
                dbOptions.UseNpgsql(
                    _postgreSqlContainer.GetConnectionString(),
                    npgsqlOptions => npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "notification")));
            services.AddScoped<INotificationRepository, NotificationRepository>();
        }
        else
        {
            services.AddScoped<INotificationRepository, NoOpNotificationRepository>();
        }

        var provider = services.BuildServiceProvider();

        return new NotificationWorker(
            provider.GetRequiredService<IServiceScopeFactory>(),
            options,
            NullLogger<NotificationWorker>.Instance);
    }

    private NotificationDbContext CreateNotificationDbContext()
    {
        var options = new DbContextOptionsBuilder<NotificationDbContext>()
            .UseNpgsql(
                _postgreSqlContainer.GetConnectionString(),
                npgsqlOptions => npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "notification"))
            .Options;

        return new NotificationDbContext(options);
    }

    private static async Task PublishTestMessageAsync(RabbitMqOptions options, string queue, string routingKey, Guid messageId, Guid notificationId)
    {
        var factory = CreateConnectionFactory(options);
        await using var connection = await factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        var payload = JsonSerializer.Serialize(new { channel = queue.Contains("email", StringComparison.OrdinalIgnoreCase) ? "Email" : "InApp", notificationId });
        var properties = new BasicProperties
        {
            Persistent = true,
            MessageId = messageId.ToString(),
            ContentType = "application/json"
        };

        await channel.BasicPublishAsync(
            exchange: RabbitMqTopologyNames.NotificationsExchange,
            routingKey: routingKey,
            mandatory: false,
            basicProperties: properties,
            body: System.Text.Encoding.UTF8.GetBytes(payload));
    }

    private sealed class NoOpNotificationRepository : INotificationRepository
    {
        public Task<DomainNotification> AddAsync(DomainNotification entity, CancellationToken cancellationToken = default) => Task.FromResult(entity);
        public Task DeleteAsync(DomainNotification entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<IEnumerable<DomainNotification>> FindAsync(System.Linq.Expressions.Expression<Func<DomainNotification, bool>> predicate, CancellationToken cancellationToken = default) => Task.FromResult(Enumerable.Empty<DomainNotification>());
        public Task<IEnumerable<DomainNotification>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult(Enumerable.Empty<DomainNotification>());
        public Task<DomainNotification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<DomainNotification?>(null);
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
        public Task UpdateAsync(DomainNotification entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<(IReadOnlyList<DomainNotification> Items, int TotalCount)> GetPagedAsync(NotificationStatus? status, NotificationChannel? channel, DateTime? createdFromUtc, DateTime? createdToUtc, int page, int pageSize, string? scopeRestriction, CancellationToken cancellationToken) => Task.FromResult(((IReadOnlyList<DomainNotification>)Array.Empty<DomainNotification>(), 0));
        public Task<DomainNotification?> GetByIdForScopeAsync(Guid id, string? scopeRestriction, CancellationToken cancellationToken) => Task.FromResult<DomainNotification?>(null);
    }
}
