using BuildingBlocks.Observability;
using BuildingBlocks.Observability.DataContext;
using Delivery.Infrastructure.Email;
using Delivery.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Delivery.Presentation
{
    /// <summary>
    /// Khởi tạo module Delivery, đăng ký dịch vụ và cấu hình khi ứng dụng khởi động.
    /// </summary>
    public class DeliveryModuleInitializer : IModuleInitializer
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
                app.ApplyMigrations<DeliveryDbContext>();
            }
        }

        /// <summary>
        /// Đăng ký các dịch vụ của module Delivery vào DI container.
        /// </summary>
        /// <param name="services">Tập hợp dịch vụ của ứng dụng.</param>
        /// <param name="configuration">Cấu hình ứng dụng.</param>
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDeliveryPersistence(configuration);
            services.AddDeliveryEmail(configuration);
        }
    }
}
