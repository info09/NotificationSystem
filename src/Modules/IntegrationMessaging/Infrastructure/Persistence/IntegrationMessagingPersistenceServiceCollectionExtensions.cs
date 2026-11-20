using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationMessaging.Infrastructure.Persistence;

public static class IntegrationMessagingPersistenceServiceCollectionExtensions
{
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

        return services;
    }
}
