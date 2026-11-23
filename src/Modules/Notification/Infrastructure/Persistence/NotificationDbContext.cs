using Microsoft.EntityFrameworkCore;
using DomainNotification = global::Notification.Domain.Notification;

namespace Notification.Infrastructure.Persistence;

/// <summary>
/// DbContext của module Notification.
/// </summary>
/// <param name="options">Tùy chọn cấu hình DbContext.</param>
public sealed class NotificationDbContext(DbContextOptions<NotificationDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Tập thực thể Thông báo.
    /// </summary>
    public DbSet<DomainNotification> Notifications => Set<DomainNotification>();

    /// <summary>
    /// Cấu hình mô hình dữ liệu cho module Notification.
    /// </summary>
    /// <param name="modelBuilder">Builder cấu hình mô hình EF Core.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("notification");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotificationDbContext).Assembly);
    }
}

