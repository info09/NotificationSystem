using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityAccess.Infrastructure.Persistence;

/// <summary>
/// Phương thức mở rộng đăng ký persistence cho module IdentityAccess.
/// </summary>
public static class IdentityAccessPersistenceServiceCollectionExtensions
{
    /// <summary>
    /// Đăng ký DbContext và các thành phần persistence của module IdentityAccess.
    /// </summary>
    /// <param name="services">Tập hợp dịch vụ của ứng dụng.</param>
    /// <param name="configuration">Cấu hình ứng dụng.</param>
    /// <returns>Tập hợp dịch vụ để chuỗi cấu hình.</returns>
    public static IServiceCollection AddIdentityAccessPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString = configuration.GetConnectionString("Database");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'Database' is not configured.");
        }

        services.AddDbContext<IdentityAccessDbContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOptions =>
                npgsqlOptions.MigrationsAssembly(typeof(IdentityAccessDbContext).Assembly.FullName)
                    .MigrationsHistoryTable("__EFMigrationsHistory", "identity_access")));

        return services;
    }
}
