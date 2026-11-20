using Microsoft.EntityFrameworkCore;
using DomainNotification = global::Notification.Domain.Notification;

namespace Notification.Infrastructure.Persistence;

public sealed class NotificationDbContext(DbContextOptions<NotificationDbContext> options) : DbContext(options)
{
    public DbSet<DomainNotification> Notifications => Set<DomainNotification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("notification");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotificationDbContext).Assembly);
    }
}

