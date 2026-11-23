using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace IdentityAccess.Infrastructure.Persistence;

/// <summary>
/// Factory tạo <see cref="IdentityAccessDbContext"/> tại thời điểm thiết kế cho EF Core.
/// </summary>
public sealed class IdentityAccessDbContextFactory
    : IDesignTimeDbContextFactory<IdentityAccessDbContext>
{
    /// <summary>
    /// Tạo DbContext dùng cho lệnh EF Core design-time.
    /// </summary>
    /// <param name="args">Tham số dòng lệnh.</param>
    /// <returns>Đối tượng <see cref="IdentityAccessDbContext"/>.</returns>
    public IdentityAccessDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Database")
            ?? "Host=localhost;Port=5432;Database=notification;Username=postgres;Password=postgres";

        var optionsBuilder = new DbContextOptionsBuilder<IdentityAccessDbContext>();
        optionsBuilder.UseNpgsql(connectionString, npgsqlOptions =>
            npgsqlOptions.MigrationsAssembly(typeof(IdentityAccessDbContext).Assembly.FullName)
                .MigrationsHistoryTable("__EFMigrationsHistory", "identity_access"));

        return new IdentityAccessDbContext(optionsBuilder.Options);
    }
}
