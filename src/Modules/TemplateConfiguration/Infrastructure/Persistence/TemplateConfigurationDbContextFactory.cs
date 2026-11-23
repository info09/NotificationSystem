using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TemplateConfiguration.Infrastructure.Persistence;

/// <summary>
/// Factory tạo <see cref="TemplateConfigurationDbContext"/> tại thời điểm thiết kế cho EF Core.
/// </summary>
public sealed class TemplateConfigurationDbContextFactory
    : IDesignTimeDbContextFactory<TemplateConfigurationDbContext>
{
    /// <summary>
    /// Tạo DbContext dùng cho lệnh EF Core design-time.
    /// </summary>
    /// <param name="args">Tham số dòng lệnh.</param>
    /// <returns>Đối tượng <see cref="TemplateConfigurationDbContext"/>.</returns>
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
