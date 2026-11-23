namespace IntegrationMessaging.Infrastructure.Messaging;

/// <summary>
/// Hằng số tên topology RabbitMQ cho Notification System.
/// </summary>
public static class RabbitMqTopologyNames
{
    /// <summary>
    /// Tên exchange topic chính cho Thông báo.
    /// </summary>
    public const string NotificationsExchange = "notifications.topic";

    /// <summary>
    /// Tên queue xử lý kênh Email.
    /// </summary>
    public const string EmailQueue = "notifications.email.q";

    /// <summary>
    /// Tên queue xử lý kênh InApp.
    /// </summary>
    public const string InAppQueue = "notifications.inapp.q";

    /// <summary>
    /// Tên dead-letter queue cho kênh Email.
    /// </summary>
    public const string EmailDeadLetterQueue = "notifications.email.dlq";

    /// <summary>
    /// Tên dead-letter queue cho kênh InApp.
    /// </summary>
    public const string InAppDeadLetterQueue = "notifications.inapp.dlq";

    /// <summary>
    /// Routing key pattern cho kênh Email.
    /// </summary>
    public const string EmailRoutingKeyPattern = "notification.email.*";

    /// <summary>
    /// Routing key pattern cho kênh InApp.
    /// </summary>
    public const string InAppRoutingKeyPattern = "notification.inapp.*";
}
