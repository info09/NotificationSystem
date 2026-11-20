using Microsoft.EntityFrameworkCore;

namespace TemplateConfiguration.Infrastructure.Persistence;

public sealed class TemplateConfigurationDbContext(
    DbContextOptions<TemplateConfigurationDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("template_configuration");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TemplateConfigurationDbContext).Assembly);
    }
}
