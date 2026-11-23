using Delivery.Domain.InAppNotifications;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Infrastructure.Persistence;

/// <summary>
/// DbContext của module Delivery.
/// </summary>
/// <param name="options">Tùy chọn cấu hình DbContext.</param>
public sealed class DeliveryDbContext(DbContextOptions<DeliveryDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Tập thực thể Thông báo trong ứng dụng.
    /// </summary>
    public DbSet<InAppNotification> InAppNotifications => Set<InAppNotification>();

    /// <summary>
    /// Cấu hình mô hình dữ liệu cho module Delivery.
    /// </summary>
    /// <param name="modelBuilder">Builder cấu hình mô hình EF Core.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("delivery");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DeliveryDbContext).Assembly);
    }
}
