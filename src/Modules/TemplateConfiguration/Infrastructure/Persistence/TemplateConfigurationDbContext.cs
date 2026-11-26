using Microsoft.EntityFrameworkCore;
using TemplateConfiguration.Domain.IntegrationEventRoutes;
using TemplateConfiguration.Domain.NotificationChannelConfigs;
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
    /// Tập thực thể Cấu hình kênh thông báo.
    /// </summary>
    public DbSet<NotificationChannelConfig> NotificationChannelConfigs => Set<NotificationChannelConfig>();

    /// <summary>
    /// Tập thực thể Định tuyến sự kiện tích hợp.
    /// </summary>
    public DbSet<IntegrationEventRoute> IntegrationEventRoutes => Set<IntegrationEventRoute>();

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
