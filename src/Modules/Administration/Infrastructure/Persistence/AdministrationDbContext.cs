using Microsoft.EntityFrameworkCore;

namespace Administration.Infrastructure.Persistence;

/// <summary>
/// DbContext của module Administration.
/// </summary>
/// <param name="options">Tùy chọn cấu hình DbContext.</param>
public sealed class AdministrationDbContext(
    DbContextOptions<AdministrationDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Cấu hình mô hình dữ liệu cho module Administration.
    /// </summary>
    /// <param name="modelBuilder">Builder cấu hình mô hình EF Core.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("administration");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AdministrationDbContext).Assembly);
    }
}
