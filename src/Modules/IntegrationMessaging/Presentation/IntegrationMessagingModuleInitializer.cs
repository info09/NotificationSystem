using BuildingBlocks.Observability;
using BuildingBlocks.Observability.DataContext;
using IntegrationMessaging.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IntegrationMessaging.Presentation;

public class IntegrationMessagingModuleInitializer : IModuleInitializer
{
    public void Configure(IServiceProvider app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.ApplyMigrations<IntegrationMessagingDbContext>();
        }
    }

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddIntegrationMessagingPersistence(configuration);
    }
}
