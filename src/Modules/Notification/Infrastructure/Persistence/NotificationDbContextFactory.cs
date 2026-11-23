using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Notification.Infrastructure.Persistence;

/// <summary>
/// Factory tạo <see cref="NotificationDbContext"/> tại thời điểm thiết kế cho EF Core.
/// </summary>
public sealed class NotificationDbContextFactory : IDesignTimeDbContextFactory<NotificationDbContext>
{
    /// <summary>
    /// Tạo DbContext dùng cho lệnh EF Core design-time.
    /// </summary>
    /// <param name="args">Tham số dòng lệnh.</param>
    /// <returns>Đối tượng <see cref="NotificationDbContext"/>.</returns>
    public NotificationDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Database")
            ?? "Host=localhost;Port=5432;Database=notification;Username=postgres;Password=postgres";

        var optionsBuilder = new DbContextOptionsBuilder<NotificationDbContext>();
        optionsBuilder.UseNpgsql(connectionString, npgsqlOptions =>
            npgsqlOptions.MigrationsAssembly(typeof(NotificationDbContext).Assembly.FullName)
                .MigrationsHistoryTable("__EFMigrationsHistory", "notification"));

        return new NotificationDbContext(optionsBuilder.Options);
    }
}
