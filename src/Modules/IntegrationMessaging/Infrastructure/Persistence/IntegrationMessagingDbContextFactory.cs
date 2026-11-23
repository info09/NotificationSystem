using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace IntegrationMessaging.Infrastructure.Persistence;

/// <summary>
/// Factory tạo <see cref="IntegrationMessagingDbContext"/> tại thời điểm thiết kế cho EF Core.
/// </summary>
public sealed class IntegrationMessagingDbContextFactory
    : IDesignTimeDbContextFactory<IntegrationMessagingDbContext>
{
    /// <summary>
    /// Tạo DbContext dùng cho lệnh EF Core design-time.
    /// </summary>
    /// <param name="args">Tham số dòng lệnh.</param>
    /// <returns>Đối tượng <see cref="IntegrationMessagingDbContext"/>.</returns>
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
