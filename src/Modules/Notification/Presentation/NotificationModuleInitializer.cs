using BuildingBlocks.Observability;
using BuildingBlocks.Observability.DataContext;
using BuildingBlocks.SharedKernel.IntegrationEvents;
using BuildingBlocks.SharedKernel.Notifications;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Notification.Infrastructure;
using Notification.Infrastructure.Persistence;

namespace Notification.Presentation;

/// <summary>
/// Khởi tạo module Notification, đăng ký dịch vụ và cấu hình khi ứng dụng khởi động.
/// </summary>
public class NotificationModuleInitializer : IModuleInitializer
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
            app.ApplyMigrations<NotificationDbContext>();
        }
    }

    /// <summary>
    /// Đăng ký các dịch vụ của module Notification vào DI container.
    /// </summary>
    /// <param name="services">Tập hợp dịch vụ của ứng dụng.</param>
    /// <param name="configuration">Cấu hình ứng dụng.</param>
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddNotificationPersistence(configuration);

        services.AddScoped<INotificationResendStateService, NotificationResendStateServiceAdapter>();
        services.AddScoped<IIntegrationNotificationCreator, IntegrationNotificationCreator>();
    }
}
