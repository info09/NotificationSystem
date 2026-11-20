using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Administration.Infrastructure.Persistence;

public sealed class AdministrationDbContextFactory
    : IDesignTimeDbContextFactory<AdministrationDbContext>
{
    public AdministrationDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Database")
            ?? "Host=localhost;Port=5432;Database=notification;Username=postgres;Password=postgres";

        var optionsBuilder = new DbContextOptionsBuilder<AdministrationDbContext>();
        optionsBuilder.UseNpgsql(connectionString, npgsqlOptions =>
            npgsqlOptions.MigrationsAssembly(typeof(AdministrationDbContext).Assembly.FullName)
                .MigrationsHistoryTable("__EFMigrationsHistory", "administration"));

        return new AdministrationDbContext(optionsBuilder.Options);
    }
}
