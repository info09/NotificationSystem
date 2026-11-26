using BuildingBlocks.Observability;
using BuildingBlocks.Observability.DataContext;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TemplateConfiguration.Domain.IntegrationEventRoutes;
using TemplateConfiguration.Domain.NotificationChannelConfigs;
using TemplateConfiguration.Domain.NotificationTemplates;
using TemplateConfiguration.Infrastructure.Persistence;

namespace TemplateConfiguration.Presentation;

/// <summary>
/// Khởi tạo module TemplateConfiguration, đăng ký dịch vụ và cấu hình khi ứng dụng khởi động.
/// </summary>
public class TemplateConfigurationModuleInitializer : IModuleInitializer
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
            app.ApplyMigrations<TemplateConfigurationDbContext>();
            SeedDevelopmentData(app);
        }
    }

    /// <summary>
    /// Seed dữ liệu phát triển mặc định cho module TemplateConfiguration.
    /// </summary>
    /// <param name="serviceProvider">Service provider của ứng dụng.</param>
    private static async Task SeedDevelopmentData(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TemplateConfigurationDbContext>();
        var now = DateTime.UtcNow;
        var changed = false;

        var templateId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var hasTemplate = dbContext.NotificationTemplates.Any(x => x.TemplateKey == "order-created-email");
        if (!hasTemplate)
        {
            await dbContext.NotificationTemplates.AddAsync(NotificationTemplate.Create(
                templateId,
                "order-created-email",
                "Thông báo tạo đơn hàng",
                "Mẫu thông báo email khi đơn hàng được tạo.",
                "Email",
                "Đơn hàng {{orderCode}} đã được tạo",
                "<p>Xin chào {{customerName}},</p><p>Đơn hàng <strong>{{orderCode}}</strong> của bạn đã được tạo lúc {{createdAt}}.</p>",
                true,
                now));
            changed = true;
        }

        var hasChannelConfig = dbContext.NotificationChannelConfigs.Any(x => x.Channel == "Email");
        if (!hasChannelConfig)
        {
            await dbContext.NotificationChannelConfigs.AddAsync(NotificationChannelConfig.Create(
                Guid.Parse("22222222-2222-2222-2222-222222222222"),
                "Email",
                "Kênh Email",
                "Cấu hình bật/tắt kênh gửi Email.",
                true,
                now));
            changed = true;
        }

        var hasRoute = dbContext.IntegrationEventRoutes.Any(x => x.EventType == "OrderCreated");
        if (!hasRoute)
        {
            await dbContext.IntegrationEventRoutes.AddAsync(IntegrationEventRoute.Create(
                Guid.Parse("33333333-3333-3333-3333-333333333333"),
                "OrderCreated",
                templateId,
                "Email",
                "Định tuyến sự kiện OrderCreated tới mẫu email tạo đơn hàng.",
                true,
                now));
            changed = true;
        }

        if (changed)
        {
            await dbContext.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Đăng ký các dịch vụ của module TemplateConfiguration vào DI container.
    /// </summary>
    /// <param name="services">Tập hợp dịch vụ của ứng dụng.</param>
    /// <param name="configuration">Cấu hình ứng dụng.</param>
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddTemplateConfigurationPersistence(configuration);
    }
}
