using IntegrationMessaging.Domain;
using IntegrationMessaging.Infrastructure.Messaging;
using IntegrationMessaging.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using Worker.Outbox;

namespace IntegrationTests.IntegrationMessaging;

/// <summary>
/// Integration test cho Outbox Publisher.
/// </summary>
public sealed class OutboxPublisherIntegrationTests : IAsyncLifetime
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
    public async Task PublishPendingMessagesAsync_WithPendingOutboxMessage_PublishesAndMarksPublished()
    {
        var options = CreateRabbitMqOptions();
        var initializer = new RabbitMqTopologyInitializer(options);
        await initializer.InitializeAsync();

        await using var dbContext = CreateDbContext();
        await dbContext.Database.EnsureCreatedAsync();

        var outboxMessage = OutboxMessage.CreatePending(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "NotificationCreated",
            JsonSerializer.Serialize(new { channel = "Email", notificationId = Guid.NewGuid() }),
            DateTime.UtcNow);

        dbContext.OutboxMessages.Add(outboxMessage);
        await dbContext.SaveChangesAsync();

        var scopeFactory = BuildScopeFactory();
        var publisher = new RabbitMqMessagePublisher(options);
        var backgroundService = new OutboxPublisherBackgroundService(
            scopeFactory,
            publisher,
            NullLogger<OutboxPublisherBackgroundService>.Instance);

        var publishedCount = await backgroundService.PublishPendingMessagesAsync(CancellationToken.None);

        Assert.Equal(1, publishedCount);

        await using var verifyContext = CreateDbContext();
        var persisted = await verifyContext.OutboxMessages.SingleAsync();
        Assert.Equal(OutboxMessageStatus.Published, persisted.Status);
        Assert.NotNull(persisted.ProcessedAtUtc);

        var factory = CreateConnectionFactory(options.Value);
        await using var connection = await factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();
        var result = await channel.BasicGetAsync(RabbitMqTopologyNames.EmailQueue, autoAck: true);

        Assert.NotNull(result);
        var body = Encoding.UTF8.GetString(result!.Body.ToArray());
        Assert.Contains("notificationId", body);
    }

    [Fact]
    public async Task PublishPendingMessagesAsync_WhenPublishFails_RecordsRetryAndKeepsOutboxMessage()
    {
        await using var dbContext = CreateDbContext();
        await dbContext.Database.EnsureCreatedAsync();

        var outboxMessage = OutboxMessage.CreatePending(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "NotificationCreated",
            JsonSerializer.Serialize(new { channel = "Email", notificationId = Guid.NewGuid() }),
            DateTime.UtcNow);

        dbContext.OutboxMessages.Add(outboxMessage);
        await dbContext.SaveChangesAsync();

        var backgroundService = new OutboxPublisherBackgroundService(
            BuildScopeFactory(),
            new ThrowingRabbitMqMessagePublisher(),
            NullLogger<OutboxPublisherBackgroundService>.Instance);

        var publishedCount = await backgroundService.PublishPendingMessagesAsync(CancellationToken.None);

        Assert.Equal(0, publishedCount);

        await using var verifyContext = CreateDbContext();
        var persisted = await verifyContext.OutboxMessages.SingleAsync();
        Assert.Equal(OutboxMessageStatus.Pending, persisted.Status);
        Assert.Equal(1, persisted.RetryCount);
        Assert.False(string.IsNullOrWhiteSpace(persisted.LastError));
        Assert.True(persisted.NextAttemptAtUtc > DateTime.UtcNow.AddSeconds(-1));
    }

    [Fact]
    public async Task PublishPendingMessagesAsync_WhenNextAttemptIsInFuture_SkipsMessage()
    {
        await using var dbContext = CreateDbContext();
        await dbContext.Database.EnsureCreatedAsync();

        var outboxMessage = OutboxMessage.CreatePending(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "NotificationCreated",
            JsonSerializer.Serialize(new { channel = "Email", notificationId = Guid.NewGuid() }),
            DateTime.UtcNow);
        outboxMessage.MarkPublishFailed("temporary", DateTime.UtcNow, DateTime.UtcNow.AddMinutes(10), maxRetryCount: 5);

        dbContext.OutboxMessages.Add(outboxMessage);
        await dbContext.SaveChangesAsync();

        var options = CreateRabbitMqOptions();
        await new RabbitMqTopologyInitializer(options).InitializeAsync();

        var backgroundService = new OutboxPublisherBackgroundService(
            BuildScopeFactory(),
            new RabbitMqMessagePublisher(options),
            NullLogger<OutboxPublisherBackgroundService>.Instance);

        var publishedCount = await backgroundService.PublishPendingMessagesAsync(CancellationToken.None);

        Assert.Equal(0, publishedCount);

        await using var verifyContext = CreateDbContext();
        var persisted = await verifyContext.OutboxMessages.SingleAsync();
        Assert.Equal(OutboxMessageStatus.Pending, persisted.Status);
        Assert.Equal(1, persisted.RetryCount);
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

    private IServiceScopeFactory BuildScopeFactory()
    {
        var services = new ServiceCollection();
        services.AddDbContext<IntegrationMessagingDbContext>(options =>
            options.UseNpgsql(
                _postgreSqlContainer.GetConnectionString(),
                npgsqlOptions => npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "integration_messaging")));

        return services.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>();
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

    private sealed class ThrowingRabbitMqMessagePublisher : IRabbitMqMessagePublisher
    {
        public Task PublishAsync(string exchange, string routingKey, string payload, Guid messageId, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("Simulated publish failure.");
        }
    }
}
