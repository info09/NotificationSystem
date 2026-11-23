using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Delivery.Infrastructure.Persistence;

/// <summary>
/// Factory tạo <see cref="DeliveryDbContext"/> tại thời điểm thiết kế cho EF Core.
/// </summary>
public sealed class DeliveryDbContextFactory : IDesignTimeDbContextFactory<DeliveryDbContext>
{
    /// <summary>
    /// Tạo DbContext dùng cho lệnh EF Core design-time.
    /// </summary>
    /// <param name="args">Tham số dòng lệnh.</param>
    /// <returns>Đối tượng <see cref="DeliveryDbContext"/>.</returns>
    public DeliveryDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Database")
            ?? "Host=localhost;Port=5432;Database=notification;Username=postgres;Password=postgres";

        var optionsBuilder = new DbContextOptionsBuilder<DeliveryDbContext>();
        optionsBuilder.UseNpgsql(connectionString, npgsqlOptions =>
            npgsqlOptions.MigrationsAssembly(typeof(DeliveryDbContext).Assembly.FullName)
                .MigrationsHistoryTable("__EFMigrationsHistory", "delivery"));

        return new DeliveryDbContext(optionsBuilder.Options);
    }
}
