using BuildingBlocks.Observability;
using BuildingBlocks.Observability.DataContext;
using IntegrationMessaging.Infrastructure.Messaging;
using IntegrationMessaging.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IntegrationMessaging.Presentation;

/// <summary>
/// Khởi tạo module IntegrationMessaging, đăng ký dịch vụ và cấu hình khi ứng dụng khởi động.
/// </summary>
public class IntegrationMessagingModuleInitializer : IModuleInitializer
{
    /// <summary>
    /// Cấu hình module khi ứng dụng khởi động.
    /// </summary>
    /// <param name="app">Service provider của ứng dụng.</param>
    /// <param name="env">Môi trường hosting hiện tại.</param>
    public void Configure(IServiceProvider app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.ApplyMigrations<IntegrationMessagingDbContext>();
        }

        InitializeTopologySafely(app);
    }

    /// <summary>
    /// Đăng ký các dịch vụ của module IntegrationMessaging vào DI container.
    /// </summary>
    /// <param name="services">Tập hợp dịch vụ của ứng dụng.</param>
    /// <param name="configuration">Cấu hình ứng dụng.</param>
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddIntegrationMessagingPersistence(configuration);
        services.AddIntegrationMessagingRabbitMq(configuration);
    }

    /// <summary>
    /// Khởi tạo topology RabbitMQ và ghi log cảnh báo nếu thất bại.
    /// </summary>
    /// <param name="app">Service provider của ứng dụng.</param>
    private static void InitializeTopologySafely(IServiceProvider app)
    {
        try
        {
            var initializer = app.GetRequiredService<IRabbitMqTopologyInitializer>();
            initializer.InitializeAsync().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            var loggerFactory = app.GetService<ILoggerFactory>();
            var logger = loggerFactory?.CreateLogger<IntegrationMessagingModuleInitializer>();
            logger?.LogWarning(ex, "Khởi tạo topology RabbitMQ thất bại. Ứng dụng sẽ tiếp tục khởi động.");
        }
    }
}
