using BuildingBlocks.Observability;
using BuildingBlocks.Observability.DataContext;
using IdentityAccess.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IdentityAccess.Presentation;

public class IdentityAccessModuleInitializer : IModuleInitializer
{
    public void Configure(IServiceProvider app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.ApplyMigrations<IdentityAccessDbContext>();
        }
    }

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentityAccessJwtBearerAuthentication(configuration);
        services.AddIdentityAccessRbacAuthorization();
        services.AddIdentityAccessPersistence(configuration);
    }
}
