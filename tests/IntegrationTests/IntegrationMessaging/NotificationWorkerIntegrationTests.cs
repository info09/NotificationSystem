using System.Text.Json;
using IntegrationMessaging.Domain;
using IntegrationMessaging.Infrastructure.Messaging;
using IntegrationMessaging.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using Worker;

namespace IntegrationTests.IntegrationMessaging;

/// <summary>
/// Integration test cho NotificationWorker.
/// </summary>
public sealed class NotificationWorkerIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer = TestcontainersConfiguration.CreatePostgreSqlBuilder().Build();
    private readonly RabbitMqContainer _rabbitMqContainer = TestcontainersConfiguration.CreateRabbitMqBuilder().Build();

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
        await using var dbContext = CreateDbContext();
        await dbContext.Database.EnsureCreatedAsync();

        var messageId = Guid.NewGuid();
        await PublishTestMessageAsync(options.Value, RabbitMqTopologyNames.EmailQueue, "notification.email.created", messageId);

        var (connection, channel, ea) = await GetDeliveryEventArgsAsync(options.Value, RabbitMqTopologyNames.EmailQueue);
        await using var _ = connection;
        await using var __ = channel;

        var worker = CreateWorker(options);
        await worker.HandleMessageAsync(channel, ea, CancellationToken.None);

        await using var verifyContext = CreateDbContext();
        var inbox = await verifyContext.InboxMessages.SingleAsync();
        Assert.Equal(messageId, inbox.MessageId);
        Assert.Equal("NotificationWorker", inbox.ConsumerName);

        var remaining = await channel.BasicGetAsync(RabbitMqTopologyNames.EmailQueue, autoAck: true);
        Assert.Null(remaining);
    }

    [Fact]
    public async Task HandleMessageAsync_WithDuplicateMessage_AcksWithoutCreatingDuplicateInbox()
    {
        var options = CreateRabbitMqOptions();
        await new RabbitMqTopologyInitializer(options).InitializeAsync();
        await using var dbContext = CreateDbContext();
        await dbContext.Database.EnsureCreatedAsync();

        var messageId = Guid.NewGuid();
        dbContext.InboxMessages.Add(InboxMessage.Create(Guid.NewGuid(), messageId, "NotificationWorker", "NotificationCreated", DateTime.UtcNow));
        await dbContext.SaveChangesAsync();

        await PublishTestMessageAsync(options.Value, RabbitMqTopologyNames.EmailQueue, "notification.email.created", messageId);

        var (connection, channel, ea) = await GetDeliveryEventArgsAsync(options.Value, RabbitMqTopologyNames.EmailQueue);
        await using var _ = connection;
        await using var __ = channel;

        var worker = CreateWorker(options);
        await worker.HandleMessageAsync(channel, ea, CancellationToken.None);

        await using var verifyContext = CreateDbContext();
        var count = await verifyContext.InboxMessages.CountAsync(x => x.MessageId == messageId);
        Assert.Equal(1, count);
    }

    private NotificationWorker CreateWorker(IOptions<RabbitMqOptions> options)
    {
        var services = new ServiceCollection();
        services.AddDbContext<IntegrationMessagingDbContext>(dbOptions =>
            dbOptions.UseNpgsql(
                _postgreSqlContainer.GetConnectionString(),
                npgsqlOptions => npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "integration_messaging")));
        var provider = services.BuildServiceProvider();

        return new NotificationWorker(
            provider.GetRequiredService<IServiceScopeFactory>(),
            options,
            NullLogger<NotificationWorker>.Instance);
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

    private IntegrationMessagingDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<IntegrationMessagingDbContext>()
            .UseNpgsql(
                _postgreSqlContainer.GetConnectionString(),
                npgsqlOptions => npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "integration_messaging"))
            .Options;

        return new IntegrationMessagingDbContext(options);
    }

    private static async Task PublishTestMessageAsync(RabbitMqOptions options, string queue, string routingKey, Guid messageId)
    {
        var factory = CreateConnectionFactory(options);
        await using var connection = await factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        var payload = JsonSerializer.Serialize(new { channel = "Email", notificationId = Guid.NewGuid() });
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
}
