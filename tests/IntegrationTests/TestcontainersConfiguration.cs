using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace IntegrationTests;

/// <summary>
/// Cấu hình khung Testcontainers/Docker cho IntegrationTests.
/// Sử dụng khi triển khai integration test cần PostgreSQL/RabbitMQ thật.
/// </summary>
public static class TestcontainersConfiguration
{
    public static PostgreSqlBuilder CreatePostgreSqlBuilder() =>
        new PostgreSqlBuilder()
            .WithImage("postgres:16")
            .WithDatabase("notification_test")
            .WithUsername("postgres")
            .WithPassword("postgres");

    public static RabbitMqBuilder CreateRabbitMqBuilder() =>
        new RabbitMqBuilder()
            .WithImage("rabbitmq:3-management")
            .WithUsername("guest")
            .WithPassword("guest");
}
