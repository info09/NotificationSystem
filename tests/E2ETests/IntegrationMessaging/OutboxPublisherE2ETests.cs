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

namespace E2ETests.IntegrationMessaging;

/// <summary>
/// E2E test cho luồng publish OutboxMessage.
/// </summary>
public sealed class OutboxPublisherE2ETests : IAsyncLifetime
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
    public async Task OutboxPublisher_PublishesPendingMessage_AndMarksAsPublished()
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
            dbContext.OutboxMessages.Add(OutboxMessage.CreatePending(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "NotificationCreated",
                JsonSerializer.Serialize(new { channel = "InApp", notificationId = Guid.NewGuid() }),
                DateTime.UtcNow));
            await dbContext.SaveChangesAsync();
        }

        var services = new ServiceCollection();
        services.AddDbContext<IntegrationMessagingDbContext>(dbOptions =>
            dbOptions.UseNpgsql(
                _postgreSqlContainer.GetConnectionString(),
                npgsqlOptions => npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "integration_messaging")));
        var provider = services.BuildServiceProvider();

        var backgroundService = new OutboxPublisherBackgroundService(
            provider.GetRequiredService<IServiceScopeFactory>(),
            new RabbitMqMessagePublisher(options),
            NullLogger<OutboxPublisherBackgroundService>.Instance);

        var publishedCount = await backgroundService.PublishPendingMessagesAsync(CancellationToken.None);

        Assert.Equal(1, publishedCount);

        await using var verifyContext = CreateDbContext();
        var persisted = await verifyContext.OutboxMessages.SingleAsync();
        Assert.Equal(OutboxMessageStatus.Published, persisted.Status);

        var factory = new ConnectionFactory
        {
            HostName = options.Value.Host,
            Port = options.Value.Port,
            UserName = options.Value.UserName,
            Password = options.Value.Password,
            VirtualHost = options.Value.VirtualHost
        };
        await using var connection = await factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();
        var result = await channel.BasicGetAsync(RabbitMqTopologyNames.InAppQueue, autoAck: true);

        Assert.NotNull(result);
        Assert.Contains("notificationId", Encoding.UTF8.GetString(result!.Body.ToArray()));
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
}
