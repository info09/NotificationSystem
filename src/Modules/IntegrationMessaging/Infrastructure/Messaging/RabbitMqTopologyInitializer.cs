using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace IntegrationMessaging.Infrastructure.Messaging;

/// <summary>
/// Khởi tạo topology RabbitMQ MVP cho Notification System.
/// </summary>
public sealed class RabbitMqTopologyInitializer : IRabbitMqTopologyInitializer
{
    private readonly RabbitMqOptions _options;

    /// <summary>
    /// Khởi tạo topology initializer với cấu hình RabbitMQ.
    /// </summary>
    /// <param name="options">Cấu hình RabbitMQ.</param>
    public RabbitMqTopologyInitializer(IOptions<RabbitMqOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options.Value;
    }

    /// <inheritdoc />
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.Host,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password,
            VirtualHost = _options.VirtualHost
        };

        await using var connection = await factory.CreateConnectionAsync(cancellationToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await DeclareTopologyAsync(channel, cancellationToken);
    }

    /// <summary>
    /// Khai báo topology RabbitMQ trên channel đã mở.
    /// </summary>
    /// <param name="channel">Channel RabbitMQ.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    public static async Task DeclareTopologyAsync(IChannel channel, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(channel);

        await channel.ExchangeDeclareAsync(
            exchange: RabbitMqTopologyNames.NotificationsExchange,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: RabbitMqTopologyNames.EmailDeadLetterQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: RabbitMqTopologyNames.InAppDeadLetterQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        var emailQueueArgs = new Dictionary<string, object?>
        {
            ["x-dead-letter-exchange"] = string.Empty,
            ["x-dead-letter-routing-key"] = RabbitMqTopologyNames.EmailDeadLetterQueue
        };

        var inAppQueueArgs = new Dictionary<string, object?>
        {
            ["x-dead-letter-exchange"] = string.Empty,
            ["x-dead-letter-routing-key"] = RabbitMqTopologyNames.InAppDeadLetterQueue
        };

        await channel.QueueDeclareAsync(
            queue: RabbitMqTopologyNames.EmailQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: emailQueueArgs,
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: RabbitMqTopologyNames.InAppQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: inAppQueueArgs,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue: RabbitMqTopologyNames.EmailQueue,
            exchange: RabbitMqTopologyNames.NotificationsExchange,
            routingKey: RabbitMqTopologyNames.EmailRoutingKeyPattern,
            arguments: null,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue: RabbitMqTopologyNames.InAppQueue,
            exchange: RabbitMqTopologyNames.NotificationsExchange,
            routingKey: RabbitMqTopologyNames.InAppRoutingKeyPattern,
            arguments: null,
            cancellationToken: cancellationToken);
    }
}
