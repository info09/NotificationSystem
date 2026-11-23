namespace Administration.Infrastructure.DeadLetterQueue;

/// <summary>
/// Cấu hình RabbitMQ dùng cho module Administration.
/// </summary>
public sealed class AdministrationRabbitMqOptions
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
    /// Port AMQP RabbitMQ.
    /// </summary>
    public int Port { get; set; } = 5672;

    /// <summary>
    /// Port HTTP Management RabbitMQ.
    /// </summary>
    public int ManagementPort { get; set; } = 15672;

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
