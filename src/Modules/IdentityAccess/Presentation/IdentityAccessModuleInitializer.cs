using BuildingBlocks.Observability;
using BuildingBlocks.Observability.DataContext;
using IdentityAccess.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IdentityAccess.Presentation;

/// <summary>
/// Khởi tạo module IdentityAccess, đăng ký xác thực/phân quyền/persistence và cấu hình khởi động.
/// </summary>
public class IdentityAccessModuleInitializer : IModuleInitializer
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
            app.ApplyMigrations<IdentityAccessDbContext>();
        }
    }

    /// <summary>
    /// Đăng ký các dịch vụ của module IdentityAccess vào DI container.
    /// </summary>
    /// <param name="services">Tập hợp dịch vụ của ứng dụng.</param>
    /// <param name="configuration">Cấu hình ứng dụng.</param>
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentityAccessJwtBearerAuthentication(configuration);
        services.AddIdentityAccessRbacAuthorization();
        services.AddIdentityAccessPersistence(configuration);
    }
}
