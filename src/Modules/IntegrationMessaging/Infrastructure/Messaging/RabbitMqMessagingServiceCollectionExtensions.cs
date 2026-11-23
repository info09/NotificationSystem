using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace IntegrationMessaging.Infrastructure.Messaging;

/// <summary>
/// Phương thức mở rộng đăng ký thành phần messaging RabbitMQ.
/// </summary>
public static class RabbitMqMessagingServiceCollectionExtensions
{
    /// <summary>
    /// Đăng ký cấu hình và dịch vụ RabbitMQ messaging.
    /// </summary>
    /// <param name="services">Tập hợp dịch vụ của ứng dụng.</param>
    /// <param name="configuration">Cấu hình ứng dụng.</param>
    /// <returns>Tập hợp dịch vụ để chuỗi cấu hình.</returns>
    public static IServiceCollection AddIntegrationMessagingRabbitMq(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOptions<RabbitMqOptions>()
            .Configure(options =>
            {
                configuration.GetSection(RabbitMqOptions.SectionName).Bind(options);
                options.Host = configuration["RABBITMQ_HOST"] ?? configuration["RabbitMq:Host"] ?? options.Host;
                options.UserName = configuration["RABBITMQ_USER"] ?? configuration["RabbitMq:UserName"] ?? options.UserName;
                options.Password = configuration["RABBITMQ_PASSWORD"] ?? configuration["RabbitMq:Password"] ?? options.Password;
                options.VirtualHost = configuration["RABBITMQ_VHOST"] ?? configuration["RabbitMq:VirtualHost"] ?? options.VirtualHost;

                var portValue = configuration["RABBITMQ_PORT"] ?? configuration["RabbitMq:Port"];
                if (int.TryParse(portValue, out var port))
                {
                    options.Port = port;
                }

                var concurrencyValue = configuration["NOTIFICATION_WORKER_CONCURRENCY"] ?? configuration["RabbitMq:ConsumerConcurrency"];
                if (int.TryParse(concurrencyValue, out var concurrency) && concurrency > 0)
                {
                    options.ConsumerConcurrency = concurrency;
                }
            });

        services.AddSingleton<IRabbitMqTopologyInitializer, RabbitMqTopologyInitializer>();
        services.AddSingleton<IRabbitMqMessagePublisher, RabbitMqMessagePublisher>();

        return services;
    }
}
