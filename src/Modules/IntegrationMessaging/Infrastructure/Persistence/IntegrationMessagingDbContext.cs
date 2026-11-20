using Microsoft.EntityFrameworkCore;

namespace IntegrationMessaging.Infrastructure.Persistence;

public sealed class IntegrationMessagingDbContext(
    DbContextOptions<IntegrationMessagingDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("integration_messaging");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IntegrationMessagingDbContext).Assembly);
    }
}
