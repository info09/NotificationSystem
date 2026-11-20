using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TemplateConfiguration.Infrastructure.Persistence;

public sealed class TemplateConfigurationDbContextFactory
    : IDesignTimeDbContextFactory<TemplateConfigurationDbContext>
{
    public TemplateConfigurationDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Database")
            ?? "Host=localhost;Port=5432;Database=notification;Username=postgres;Password=postgres";

        var optionsBuilder = new DbContextOptionsBuilder<TemplateConfigurationDbContext>();
        optionsBuilder.UseNpgsql(connectionString, npgsqlOptions =>
            npgsqlOptions.MigrationsAssembly(typeof(TemplateConfigurationDbContext).Assembly.FullName)
                .MigrationsHistoryTable("__EFMigrationsHistory", "template_configuration"));

        return new TemplateConfigurationDbContext(optionsBuilder.Options);
    }
}
