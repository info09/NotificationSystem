using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TemplateConfiguration.Application.Abstractions;
using TemplateConfiguration.Application.IntegrationEventRoutes;
using TemplateConfiguration.Application.NotificationChannelConfigs;
using TemplateConfiguration.Application.NotificationTemplates;
using TemplateConfiguration.Infrastructure.Persistence.Repositories;

namespace TemplateConfiguration.Infrastructure.Persistence;

/// <summary>
/// Phương thức mở rộng đăng ký persistence cho module TemplateConfiguration.
/// </summary>
public static class TemplateConfigurationPersistenceServiceCollectionExtensions
{
    /// <summary>
    /// Đăng ký DbContext và các thành phần persistence của module TemplateConfiguration.
    /// </summary>
    /// <param name="services">Tập hợp dịch vụ của ứng dụng.</param>
    /// <param name="configuration">Cấu hình ứng dụng.</param>
    /// <returns>Tập hợp dịch vụ để chuỗi cấu hình.</returns>
    public static IServiceCollection AddTemplateConfigurationPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString = configuration.GetConnectionString("Database");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'Database' is not configured.");
        }

        services.AddDbContext<TemplateConfigurationDbContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOptions =>
                npgsqlOptions.MigrationsAssembly(typeof(TemplateConfigurationDbContext).Assembly.FullName)
                    .MigrationsHistoryTable("__EFMigrationsHistory", "template_configuration")));

        services.AddScoped<INotificationTemplateRepository, NotificationTemplateRepository>();
        services.AddScoped<CreateNotificationTemplateUseCase>();
        services.AddScoped<UpdateNotificationTemplateUseCase>();
        services.AddScoped<GetNotificationTemplateByIdUseCase>();
        services.AddScoped<GetNotificationTemplatesUseCase>();
        services.AddScoped<DeleteNotificationTemplateUseCase>();

        services.AddScoped<INotificationChannelConfigRepository, NotificationChannelConfigRepository>();
        services.AddScoped<CreateNotificationChannelConfigUseCase>();
        services.AddScoped<UpdateNotificationChannelConfigUseCase>();
        services.AddScoped<GetNotificationChannelConfigByIdUseCase>();
        services.AddScoped<GetNotificationChannelConfigsUseCase>();
        services.AddScoped<DeleteNotificationChannelConfigUseCase>();

        services.AddScoped<IIntegrationEventRouteRepository, IntegrationEventRouteRepository>();
        services.AddScoped<CreateIntegrationEventRouteUseCase>();
        services.AddScoped<UpdateIntegrationEventRouteUseCase>();
        services.AddScoped<GetIntegrationEventRouteByIdUseCase>();
        services.AddScoped<GetIntegrationEventRoutesUseCase>();
        services.AddScoped<DeleteIntegrationEventRouteUseCase>();

        return services;
    }
}
