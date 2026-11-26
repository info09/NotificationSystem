using Microsoft.EntityFrameworkCore;
using TemplateConfiguration.Domain.NotificationTemplates;

namespace TemplateConfiguration.Infrastructure.Persistence;

/// <summary>
/// DbContext của module TemplateConfiguration.
/// </summary>
/// <param name="options">Tùy chọn cấu hình DbContext.</param>
public sealed class TemplateConfigurationDbContext(
    DbContextOptions<TemplateConfigurationDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Tập thực thể Mẫu thông báo.
    /// </summary>
    public DbSet<NotificationTemplate> NotificationTemplates => Set<NotificationTemplate>();

    /// <summary>
    /// Cấu hình mô hình dữ liệu cho module TemplateConfiguration.
    /// </summary>
    /// <param name="modelBuilder">Builder cấu hình mô hình EF Core.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("template_configuration");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TemplateConfigurationDbContext).Assembly);
    }
}
