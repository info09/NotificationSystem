using BuildingBlocks.Observability;
using BuildingBlocks.Observability.DataContext;
using Delivery.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Delivery.Presentation
{
    public class DeliveryModuleInitializer : IModuleInitializer
    {
        public void Configure(IServiceProvider app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.ApplyMigrations<DeliveryDbContext>();
            }
        }

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDeliveryPersistence(configuration);
        }
    }
}
