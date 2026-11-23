using Microsoft.EntityFrameworkCore;

namespace IdentityAccess.Infrastructure.Persistence;

/// <summary>
/// DbContext của module IdentityAccess.
/// </summary>
/// <param name="options">Tùy chọn cấu hình DbContext.</param>
public sealed class IdentityAccessDbContext(
    DbContextOptions<IdentityAccessDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Cấu hình mô hình dữ liệu cho module IdentityAccess.
    /// </summary>
    /// <param name="modelBuilder">Builder cấu hình mô hình EF Core.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("identity_access");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityAccessDbContext).Assembly);
    }
}
