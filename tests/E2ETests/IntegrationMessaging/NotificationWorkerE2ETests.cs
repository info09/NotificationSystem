using System.Text.Json;
using Delivery.Application.Abstractions;
using Delivery.Application.Email;
using Delivery.Infrastructure.Persistence;
using IntegrationMessaging.Infrastructure.Messaging;
using IntegrationMessaging.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
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
    public async Task HandleMessageAsync_WithInvalidMessageId_RoutesToDlq()
    {
        var options = CreateRabbitMqOptions();
        await new RabbitMqTopologyInitializer(options).InitializeAsync();
        await using (var dbContext = CreateDbContext())
        {
            await dbContext.Database.EnsureCreatedAsync();
        }

        await PublishRawMessageAsync(options.Value, "notification.email.created", "not-a-guid");

        var (connection, channel, ea) = await GetDeliveryEventArgsAsync(options.Value, RabbitMqTopologyNames.EmailQueue);
        await using var _ = connection;
        await using var __ = channel;

        var worker = CreateWorker(options);
        await worker.HandleMessageAsync(channel, ea, CancellationToken.None);

        BasicGetResult? dlqMessage = null;
        for (var i = 0; i < 20 && dlqMessage is null; i++)
        {
            dlqMessage = await channel.BasicGetAsync(RabbitMqTopologyNames.EmailDeadLetterQueue, autoAck: true);
            if (dlqMessage is null)
            {
                await Task.Delay(100);
            }
        }

        Assert.NotNull(dlqMessage);
    }

    [Fact]
    public async Task HandleMessageAsync_WithRetryableException_RetriesAndRoutesToDlq()
    {
        var options = CreateRabbitMqOptions();
        await new RabbitMqTopologyInitializer(options).InitializeAsync();
        await using (var dbContext = CreateDbContext())
        {
            await dbContext.Database.EnsureCreatedAsync();
        }

        var messageId = Guid.NewGuid();
        await PublishTestMessageAsync(options.Value, RabbitMqTopologyNames.EmailQueue, "notification.email.created", messageId, Guid.NewGuid());

        var (connection, channel, ea) = await GetDeliveryEventArgsAsync(options.Value, RabbitMqTopologyNames.EmailQueue);
        await using var _ = connection;
        await using var __ = channel;

        var repository = new ThrowingNotificationRepository(new TimeoutException("temporary"));
        var worker = CreateWorker(options);
        worker = new NotificationWorker(
            new ServiceCollection()
                .AddDbContext<IntegrationMessagingDbContext>(dbOptions => dbOptions.UseNpgsql(_postgreSqlContainer.GetConnectionString(), npgsqlOptions => npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "integration_messaging")))
                .AddScoped<INotificationRepository>(_ => repository)
                .BuildServiceProvider()
                .GetRequiredService<IServiceScopeFactory>(),
            options,
            NullLogger<NotificationWorker>.Instance)
        { RetryDelaySelector = _ => TimeSpan.Zero };

        await worker.HandleMessageAsync(channel, ea, CancellationToken.None);

        BasicGetResult? dlqMessage = null;
        for (var i = 0; i < 20 && dlqMessage is null; i++)
        {
            dlqMessage = await channel.BasicGetAsync(RabbitMqTopologyNames.EmailDeadLetterQueue, autoAck: true);
            if (dlqMessage is null)
            {
                await Task.Delay(100);
            }
        }

        Assert.NotNull(dlqMessage);
        Assert.Equal(4, repository.GetByIdCallCount);
    }

    [Fact]
    public async Task HandleMessageAsync_WithMultipleMessages_ProcessesAllAndCreatesInboxRecords()
    {
        var options = CreateRabbitMqOptions();
        await new RabbitMqTopologyInitializer(options).InitializeAsync();
        await using (var dbContext = CreateDbContext())
        {
            await dbContext.Database.EnsureCreatedAsync();
        }

        var messageIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        foreach (var messageId in messageIds)
        {
            await PublishTestMessageAsync(options.Value, RabbitMqTopologyNames.EmailQueue, "notification.email.created", messageId, Guid.NewGuid());
        }

        var worker = CreateWorker(options);
        var tasks = new List<Task>();
        var connections = new List<IConnection>();
        var channels = new List<IChannel>();

        foreach (var _ in messageIds)
        {
            var (connection, channel, ea) = await GetDeliveryEventArgsAsync(options.Value, RabbitMqTopologyNames.EmailQueue);
            connections.Add(connection);
            channels.Add(channel);
            tasks.Add(worker.HandleMessageAsync(channel, ea, CancellationToken.None));
        }

        await Task.WhenAll(tasks);

        foreach (var channel in channels)
        {
            await channel.DisposeAsync();
        }

        foreach (var connection in connections)
        {
            await connection.DisposeAsync();
        }

        await using var verifyContext = CreateDbContext();
        var inboxCount = await verifyContext.InboxMessages.CountAsync();
        Assert.Equal(messageIds.Length, inboxCount);
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

    [Fact]
    public async Task HandleMessageAsync_WithInAppNotification_CreatesInAppNotification()
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
        await using (var deliveryDbContext = CreateDeliveryDbContext())
        {
            await deliveryDbContext.Database.MigrateAsync();
        }

        var notification = DomainNotification.Create(
            Guid.NewGuid(),
            new global::Notification.Domain.Recipient("user-1"),
            NotificationChannel.InApp,
            "Xin chao in-app",
            null,
            "scope-inapp-e2e",
            "key-inapp-e2e",
            DateTime.UtcNow);
        await using (var notificationDbContext = CreateNotificationDbContext())
        {
            notificationDbContext.Notifications.Add(notification);
            await notificationDbContext.SaveChangesAsync();
        }

        var messageId = Guid.NewGuid();
        await PublishTestMessageAsync(options.Value, RabbitMqTopologyNames.InAppQueue, "notification.inapp.created", messageId, notification.Id);

        var (consumeConnection, consumeChannel, ea) = await GetDeliveryEventArgsAsync(options.Value, RabbitMqTopologyNames.InAppQueue);
        await using var _ = consumeConnection;
        await using var __ = consumeChannel;

        var worker = CreateWorkerWithRealDeliveryDispatch(options);
        await worker.HandleMessageAsync(consumeChannel, ea, CancellationToken.None);

        await using var verifyDeliveryDbContext = CreateDeliveryDbContext();
        var inApp = await verifyDeliveryDbContext.InAppNotifications.AsNoTracking()
            .SingleAsync(x => x.NotificationId == notification.Id && x.RecipientUserId == "user-1");
        Assert.Equal(global::Delivery.Domain.InAppNotifications.ReadStatus.Unread, inApp.ReadStatus);
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

        services.AddScoped<INotificationChannelDispatcher, NoOpNotificationChannelDispatcher>();

        var provider = services.BuildServiceProvider();

        return new NotificationWorker(
            provider.GetRequiredService<IServiceScopeFactory>(),
            options,
            NullLogger<NotificationWorker>.Instance);
    }

    private NotificationWorker CreateWorkerWithRealDeliveryDispatch(IOptions<RabbitMqOptions> options)
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Database"] = _postgreSqlContainer.GetConnectionString()
            })
            .Build();

        services.AddSingleton<IConfiguration>(configuration);
        services.AddDbContext<IntegrationMessagingDbContext>(dbOptions =>
            dbOptions.UseNpgsql(
                _postgreSqlContainer.GetConnectionString(),
                npgsqlOptions => npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "integration_messaging")));
        services.AddDbContext<NotificationDbContext>(dbOptions =>
            dbOptions.UseNpgsql(
                _postgreSqlContainer.GetConnectionString(),
                npgsqlOptions => npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "notification")));
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddDeliveryPersistence(configuration);
        services.AddScoped<IEmailSender, NoOpEmailSender>();

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

    private DeliveryDbContext CreateDeliveryDbContext()
    {
        var options = new DbContextOptionsBuilder<DeliveryDbContext>()
            .UseNpgsql(
                _postgreSqlContainer.GetConnectionString(),
                npgsqlOptions => npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "delivery"))
            .Options;

        return new DeliveryDbContext(options);
    }

    private static async Task PublishRawMessageAsync(RabbitMqOptions options, string routingKey, string messageId)
    {
        var factory = CreateConnectionFactory(options);
        await using var connection = await factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        var properties = new BasicProperties
        {
            Persistent = true,
            MessageId = messageId,
            ContentType = "application/json"
        };

        await channel.BasicPublishAsync(
            exchange: RabbitMqTopologyNames.NotificationsExchange,
            routingKey: routingKey,
            mandatory: false,
            basicProperties: properties,
            body: System.Text.Encoding.UTF8.GetBytes("""{"channel":"Email","notificationId":"00000000-0000-0000-0000-000000000001"}"""));
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

    private sealed class ThrowingNotificationRepository : INotificationRepository
    {
        private readonly Exception _exception;

        public ThrowingNotificationRepository(Exception exception)
        {
            _exception = exception;
        }

        public int GetByIdCallCount { get; private set; }

        public Task<DomainNotification> AddAsync(DomainNotification entity, CancellationToken cancellationToken = default) => Task.FromResult(entity);
        public Task DeleteAsync(DomainNotification entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<IEnumerable<DomainNotification>> FindAsync(System.Linq.Expressions.Expression<Func<DomainNotification, bool>> predicate, CancellationToken cancellationToken = default) => Task.FromResult(Enumerable.Empty<DomainNotification>());
        public Task<IEnumerable<DomainNotification>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult(Enumerable.Empty<DomainNotification>());
        public Task<DomainNotification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            GetByIdCallCount++;
            throw _exception;
        }
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
        public Task UpdateAsync(DomainNotification entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<(IReadOnlyList<DomainNotification> Items, int TotalCount)> GetPagedAsync(NotificationStatus? status, NotificationChannel? channel, DateTime? createdFromUtc, DateTime? createdToUtc, int page, int pageSize, string? scopeRestriction, CancellationToken cancellationToken) => Task.FromResult(((IReadOnlyList<DomainNotification>)Array.Empty<DomainNotification>(), 0));
        public Task<DomainNotification?> GetByIdForScopeAsync(Guid id, string? scopeRestriction, CancellationToken cancellationToken) => Task.FromResult<DomainNotification?>(null);
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

    private sealed class NoOpNotificationChannelDispatcher : INotificationChannelDispatcher
    {
        public Task DispatchAsync(NotificationChannelDispatchRequest request, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class NoOpEmailSender : IEmailSender
    {
        public Task SendAsync(SendEmailRequest request, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
