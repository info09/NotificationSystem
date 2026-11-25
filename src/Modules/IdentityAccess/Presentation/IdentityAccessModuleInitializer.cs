using BuildingBlocks.Observability;
using BuildingBlocks.Observability.DataContext;
using IdentityAccess.Application.Abstractions;
using IdentityAccess.Domain.UserAccounts;
using IdentityAccess.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
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
            SeedDevelopmentData(app);
        }
    }

    /// <summary>
    /// Seed dữ liệu phát triển mặc định cho module IdentityAccess.
    /// </summary>
    /// <param name="serviceProvider">Service provider của ứng dụng.</param>
    private static void SeedDevelopmentData(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityAccessDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<ICredentialHasher>();

        var hasAdmin = dbContext.UserAccounts
            .AsNoTracking()
            .Any(x => x.Role == UserRole.SystemAdministrator && x.Status != UserAccountStatus.Deleted);

        if (hasAdmin)
        {
            return;
        }

        var now = DateTime.UtcNow;
        var admin = UserAccount.Create(
            Guid.Parse("00000000-0000-0000-0000-000000000001"),
            "admin",
            "admin@local.dev",
            "System Administrator",
            UserRole.SystemAdministrator,
            hasher.Hash("Admin@123"),
            now);

        dbContext.UserAccounts.Add(admin);
        dbContext.SaveChanges();
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
