using BuildingBlocks.Observability;
using BuildingBlocks.Observability.DataContext;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TemplateConfiguration.Infrastructure.Persistence;

namespace TemplateConfiguration.Presentation;

public class TemplateConfigurationModuleInitializer : IModuleInitializer
{
    public void Configure(IServiceProvider app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.ApplyMigrations<TemplateConfigurationDbContext>();
        }
    }

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddTemplateConfigurationPersistence(configuration);
    }
}
