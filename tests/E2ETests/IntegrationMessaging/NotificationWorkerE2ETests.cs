using System.Text.Json;
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
        var options = Options.Create(new RabbitMqOptions
        {
            Host = _rabbitMqContainer.Hostname,
            Port = _rabbitMqContainer.GetMappedPublicPort(5672),
            UserName = "guest",
            Password = "guest",
            VirtualHost = "/"
        });

        await new RabbitMqTopologyInitializer(options).InitializeAsync();
        await using (var dbContext = CreateDbContext())
        {
            await dbContext.Database.EnsureCreatedAsync();
        }

        var messageId = Guid.NewGuid();
        var factory = CreateConnectionFactory(options.Value);
        await using (var connection = await factory.CreateConnectionAsync())
        await using (var channel = await connection.CreateChannelAsync())
        {
            var payload = JsonSerializer.Serialize(new { channel = "InApp", notificationId = Guid.NewGuid() });
            var properties = new BasicProperties
            {
                Persistent = true,
                MessageId = messageId.ToString(),
                ContentType = "application/json"
            };

            await channel.BasicPublishAsync(
                exchange: RabbitMqTopologyNames.NotificationsExchange,
                routingKey: "notification.inapp.created",
                mandatory: false,
                basicProperties: properties,
                body: System.Text.Encoding.UTF8.GetBytes(payload));
        }

        var (consumeConnection, consumeChannel, ea) = await GetDeliveryEventArgsAsync(options.Value, RabbitMqTopologyNames.InAppQueue);
        await using var _ = consumeConnection;
        await using var __ = consumeChannel;

        var services = new ServiceCollection();
        services.AddDbContext<IntegrationMessagingDbContext>(dbOptions =>
            dbOptions.UseNpgsql(
                _postgreSqlContainer.GetConnectionString(),
                npgsqlOptions => npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "integration_messaging")));
        var provider = services.BuildServiceProvider();

        var worker = new NotificationWorker(
            provider.GetRequiredService<IServiceScopeFactory>(),
            options,
            NullLogger<NotificationWorker>.Instance);

        await worker.HandleMessageAsync(consumeChannel, ea, CancellationToken.None);

        await using var verifyContext = CreateDbContext();
        var inbox = await verifyContext.InboxMessages.SingleAsync();
        Assert.Equal(messageId, inbox.MessageId);
        Assert.Equal("NotificationWorker", inbox.ConsumerName);
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
}
