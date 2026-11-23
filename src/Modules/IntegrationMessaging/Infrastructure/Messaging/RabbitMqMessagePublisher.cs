using System.Text;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace IntegrationMessaging.Infrastructure.Messaging;

/// <summary>
/// Publisher gửi Thông điệp qua RabbitMQ bằng RabbitMQ.Client.
/// </summary>
public sealed class RabbitMqMessagePublisher : IRabbitMqMessagePublisher
{
    private readonly RabbitMqOptions _options;

    /// <summary>
    /// Khởi tạo publisher với cấu hình RabbitMQ.
    /// </summary>
    /// <param name="options">Cấu hình RabbitMQ.</param>
    public RabbitMqMessagePublisher(IOptions<RabbitMqOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options.Value;
    }

    /// <inheritdoc />
    public async Task PublishAsync(string exchange, string routingKey, string payload, Guid messageId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(exchange);
        ArgumentException.ThrowIfNullOrWhiteSpace(routingKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(payload);

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

        var properties = new BasicProperties
        {
            Persistent = true,
            MessageId = messageId.ToString(),
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent
        };

        var body = Encoding.UTF8.GetBytes(payload);
        await channel.BasicPublishAsync(
            exchange: exchange,
            routingKey: routingKey,
            mandatory: false,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken);
    }
}
