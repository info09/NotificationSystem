using System.Text;
using System.Text.Json;
using IntegrationMessaging.Domain;
using IntegrationMessaging.Infrastructure.Messaging;
using IntegrationMessaging.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using Worker;

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
}
