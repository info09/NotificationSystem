using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Administration.Infrastructure.Persistence;

/// <summary>
/// Factory tạo <see cref="AdministrationDbContext"/> tại thời điểm thiết kế cho EF Core.
/// </summary>
public sealed class AdministrationDbContextFactory
    : IDesignTimeDbContextFactory<AdministrationDbContext>
{
    /// <summary>
    /// Tạo DbContext dùng cho lệnh EF Core design-time.
    /// </summary>
    /// <param name="args">Tham số dòng lệnh.</param>
    /// <returns>Đối tượng <see cref="AdministrationDbContext"/>.</returns>
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
