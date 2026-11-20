using Administration.Infrastructure.Persistence;
using BuildingBlocks.Observability;
using BuildingBlocks.Observability.DataContext;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Administration.Presentation;

public class AdministrationModuleInitializer : IModuleInitializer
{
    public void Configure(IServiceProvider app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.ApplyMigrations<AdministrationDbContext>();
        }
    }

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddAdministrationPersistence(configuration);
    }
}
