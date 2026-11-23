namespace IntegrationMessaging.Infrastructure.Messaging;

/// <summary>
/// Publisher gửi Thông điệp qua RabbitMQ.
/// </summary>
public interface IRabbitMqMessagePublisher
{
    /// <summary>
    /// Publish một Thông điệp tới exchange với routing key.
    /// </summary>
    /// <param name="exchange">Tên exchange.</param>
    /// <param name="routingKey">Routing key.</param>
    /// <param name="payload">Nội dung Thông điệp.</param>
    /// <param name="messageId">Mã định danh Thông điệp.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    Task PublishAsync(string exchange, string routingKey, string payload, Guid messageId, CancellationToken cancellationToken = default);
}
