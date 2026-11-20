using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace IntegrationMessaging.Infrastructure.Persistence;

public sealed class IntegrationMessagingDbContextFactory
    : IDesignTimeDbContextFactory<IntegrationMessagingDbContext>
{
    public IntegrationMessagingDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Database")
            ?? "Host=localhost;Port=5432;Database=notification;Username=postgres;Password=postgres";

        var optionsBuilder = new DbContextOptionsBuilder<IntegrationMessagingDbContext>();
        optionsBuilder.UseNpgsql(connectionString, npgsqlOptions =>
            npgsqlOptions.MigrationsAssembly(typeof(IntegrationMessagingDbContext).Assembly.FullName)
                .MigrationsHistoryTable("__EFMigrationsHistory", "integration_messaging"));

        return new IntegrationMessagingDbContext(optionsBuilder.Options);
    }
}
