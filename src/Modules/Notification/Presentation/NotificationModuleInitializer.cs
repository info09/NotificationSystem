using BuildingBlocks.Observability;
using BuildingBlocks.Observability.DataContext;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Notification.Infrastructure.Persistence;

namespace Notification.Presentation;

public class NotificationModuleInitializer : IModuleInitializer
{
    public void Configure(IServiceProvider app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.ApplyMigrations<NotificationDbContext>();
        }
    }

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddNotificationPersistence(configuration);
    }
}
