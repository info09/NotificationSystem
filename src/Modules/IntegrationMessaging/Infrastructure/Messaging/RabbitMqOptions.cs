namespace IntegrationMessaging.Infrastructure.Messaging;

/// <summary>
/// Cấu hình kết nối RabbitMQ.
/// </summary>
public sealed class RabbitMqOptions
{
    /// <summary>
    /// Khóa cấu hình RabbitMQ.
    /// </summary>
    public const string SectionName = "RabbitMq";

    /// <summary>
    /// Host RabbitMQ.
    /// </summary>
    public string Host { get; set; } = "localhost";

    /// <summary>
    /// Port RabbitMQ.
    /// </summary>
    public int Port { get; set; } = 5672;

    /// <summary>
    /// Tên đăng nhập RabbitMQ.
    /// </summary>
    public string UserName { get; set; } = "guest";

    /// <summary>
    /// Mật khẩu RabbitMQ.
    /// </summary>
    public string Password { get; set; } = "guest";

    /// <summary>
    /// Virtual host RabbitMQ.
    /// </summary>
    public string VirtualHost { get; set; } = "/";
}
