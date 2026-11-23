using IntegrationMessaging.Infrastructure.Messaging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Testcontainers.RabbitMq;

namespace IntegrationTests.IntegrationMessaging;

/// <summary>
/// Integration test xác minh topology RabbitMQ MVP.
/// </summary>
public sealed class RabbitMqTopologyIntegrationTests : IAsyncLifetime
{
    private readonly RabbitMqContainer _rabbitMqContainer = TestcontainersConfiguration.CreateRabbitMqBuilder().Build();

    /// <summary>
    /// Khởi động container RabbitMQ.
    /// </summary>
    public Task InitializeAsync() => _rabbitMqContainer.StartAsync();

    /// <summary>
    /// Dọn dẹp container RabbitMQ.
    /// </summary>
    public Task DisposeAsync() => _rabbitMqContainer.DisposeAsync().AsTask();

    [Fact]
    public async Task InitializeAsync_DeclaresMvpTopology()
    {
        var options = Options.Create(new RabbitMqOptions
        {
            Host = _rabbitMqContainer.Hostname,
            Port = _rabbitMqContainer.GetMappedPublicPort(5672),
            UserName = "guest",
            Password = "guest",
            VirtualHost = "/"
        });

        var initializer = new RabbitMqTopologyInitializer(options);
        await initializer.InitializeAsync();

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

        await channel.ExchangeDeclarePassiveAsync(RabbitMqTopologyNames.NotificationsExchange);
        await channel.QueueDeclarePassiveAsync(RabbitMqTopologyNames.EmailQueue);
        await channel.QueueDeclarePassiveAsync(RabbitMqTopologyNames.InAppQueue);
        await channel.QueueDeclarePassiveAsync(RabbitMqTopologyNames.EmailDeadLetterQueue);
        await channel.QueueDeclarePassiveAsync(RabbitMqTopologyNames.InAppDeadLetterQueue);
    }
}
