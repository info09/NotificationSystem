using Administration.Presentation;
using BuildingBlocks.Observability;
using Delivery.Presentation;
using IdentityAccess.Presentation;
using IntegrationMessaging.Presentation;
using Notification.Presentation;
using TemplateConfiguration.Presentation;

namespace Api.Modules;

/// <summary>
/// Lớp tiện ích đăng ký tất cả module nghiệp vụ vào DI container.
/// Gọi từ Program.cs khi khởi động ứng dụng.
/// </summary>
public static class ModuleServiceCollectionExtensions
{
    /// <summary>
    /// Đăng ký dịch vụ cross-cutting và tất cả module vào IServiceCollection.
    /// </summary>
    /// <param name="services">Tập hợp dịch vụ của ứng dụng.</param>
    /// <param name="configuration">Cấu hình ứng dụng.</param>
    public static IServiceCollection AddModules(
        this IServiceCollection services,
        IConfiguration configuration)
    {

        // Đăng ký IHttpContextAccessor để lấy userId từ JWT token trong handler
        //services.AddHttpContextAccessor();

        //services.AddScoped(typeof(IRepository<>), typeof(RepositoryBase<>));

        //services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Danh sách các module initializer, gọi ConfigureServices cho mỗi module
        IModuleInitializer[] modules =
        [
            new AdministrationModuleInitializer(),
            new DeliveryModuleInitializer(),
            new IdentityAccessModuleInitializer(),
            new IntegrationMessagingModuleInitializer(),
            new NotificationModuleInitializer(),
            new TemplateConfigurationModuleInitializer()
        ];

        foreach (var module in modules)
        {
            module.ConfigureServices(services, configuration);
            services.AddSingleton(module);
        }

        return services;
    }
}

