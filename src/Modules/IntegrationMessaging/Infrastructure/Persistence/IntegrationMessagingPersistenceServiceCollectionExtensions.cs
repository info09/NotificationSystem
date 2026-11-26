using IntegrationMessaging.Application.Abstractions;
using IntegrationMessaging.Application.IntegrationEvents;
using IntegrationMessaging.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationMessaging.Infrastructure.Persistence;

/// <summary>
/// Phương thức mở rộng đăng ký persistence cho module IntegrationMessaging.
/// </summary>
public static class IntegrationMessagingPersistenceServiceCollectionExtensions
{
    /// <summary>
    /// Đăng ký DbContext và các thành phần persistence của module IntegrationMessaging.
    /// </summary>
    /// <param name="services">Tập hợp dịch vụ của ứng dụng.</param>
    /// <param name="configuration">Cấu hình ứng dụng.</param>
    /// <returns>Tập hợp dịch vụ để chuỗi cấu hình.</returns>
    public static IServiceCollection AddIntegrationMessagingPersistence(
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

        services.AddDbContext<IntegrationMessagingDbContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOptions =>
                npgsqlOptions.MigrationsAssembly(typeof(IntegrationMessagingDbContext).Assembly.FullName)
                    .MigrationsHistoryTable("__EFMigrationsHistory", "integration_messaging")));

        services.AddScoped<IIntegrationEventRepository, IntegrationEventRepository>();
        services.AddScoped<ReceiveIntegrationEventUseCase>();

        return services;
    }
}
