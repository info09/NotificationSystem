using IntegrationMessaging.Domain;
using Microsoft.EntityFrameworkCore;

namespace IntegrationMessaging.Infrastructure.Persistence;

/// <summary>
/// DbContext của module IntegrationMessaging.
/// </summary>
/// <param name="options">Tùy chọn cấu hình DbContext.</param>
public sealed class IntegrationMessagingDbContext(
    DbContextOptions<IntegrationMessagingDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Tập thực thể Bản ghi Outbox.
    /// </summary>
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    /// <summary>
    /// Tập thực thể Bản ghi Inbox.
    /// </summary>
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    /// <summary>
    /// Cấu hình mô hình dữ liệu cho module IntegrationMessaging.
    /// </summary>
    /// <param name="modelBuilder">Builder cấu hình mô hình EF Core.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("integration_messaging");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IntegrationMessagingDbContext).Assembly);
    }
}
