using Microsoft.EntityFrameworkCore;

namespace IdentityAccess.Infrastructure.Persistence;

public sealed class IdentityAccessDbContext(
    DbContextOptions<IdentityAccessDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("identity_access");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityAccessDbContext).Assembly);
    }
}
