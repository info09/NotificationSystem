using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Infrastructure.Persistence;

/// <summary>
/// Phương thức mở rộng đăng ký persistence cho module Delivery.
/// </summary>
public static class DeliveryPersistenceServiceCollectionExtensions
{
    /// <summary>
    /// Đăng ký DbContext và các thành phần persistence của module Delivery.
    /// </summary>
    /// <param name="services">Tập hợp dịch vụ của ứng dụng.</param>
    /// <param name="configuration">Cấu hình ứng dụng.</param>
    /// <returns>Tập hợp dịch vụ để chuỗi cấu hình.</returns>
    public static IServiceCollection AddDeliveryPersistence(
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

        services.AddDbContext<DeliveryDbContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOptions =>
                npgsqlOptions.MigrationsAssembly(typeof(DeliveryDbContext).Assembly.FullName)
                    .MigrationsHistoryTable("__EFMigrationsHistory", "delivery")));

        return services;
    }
}
