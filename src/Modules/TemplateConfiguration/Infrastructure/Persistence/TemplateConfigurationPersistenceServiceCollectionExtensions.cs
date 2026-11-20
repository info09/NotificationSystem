using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TemplateConfiguration.Infrastructure.Persistence;

public static class TemplateConfigurationPersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddTemplateConfigurationPersistence(
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

        services.AddDbContext<TemplateConfigurationDbContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOptions =>
                npgsqlOptions.MigrationsAssembly(typeof(TemplateConfigurationDbContext).Assembly.FullName)
                    .MigrationsHistoryTable("__EFMigrationsHistory", "template_configuration")));

        return services;
    }
}
