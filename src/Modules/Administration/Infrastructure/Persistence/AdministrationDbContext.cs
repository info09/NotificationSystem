using Microsoft.EntityFrameworkCore;

namespace Administration.Infrastructure.Persistence;

public sealed class AdministrationDbContext(
    DbContextOptions<AdministrationDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("administration");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AdministrationDbContext).Assembly);
    }
}
