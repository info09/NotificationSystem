namespace IntegrationMessaging.Infrastructure.Messaging;

/// <summary>
/// Khởi tạo topology RabbitMQ cho hệ thống.
/// </summary>
public interface IRabbitMqTopologyInitializer
{
    /// <summary>
    /// Khai báo exchange, queue, binding và DLQ cần thiết.
    /// </summary>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    Task InitializeAsync(CancellationToken cancellationToken = default);
}
