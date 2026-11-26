using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TemplateConfiguration.Domain.IntegrationEventRoutes;

namespace TemplateConfiguration.Infrastructure.Persistence.Configurations;

/// <summary>
/// Cấu hình ánh xạ EF Core cho thực thể Định tuyến sự kiện tích hợp.
/// </summary>
public sealed class IntegrationEventRouteConfiguration : IEntityTypeConfiguration<IntegrationEventRoute>
{
    /// <summary>
    /// Cấu hình bảng, cột và chỉ mục cho Định tuyến sự kiện tích hợp.
    /// </summary>
    /// <param name="builder">Builder cấu hình thực thể.</param>
    public void Configure(EntityTypeBuilder<IntegrationEventRoute> builder)
    {
        builder.ToTable("IntegrationEventRoutes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("Id").ValueGeneratedNever();
        builder.Property(x => x.EventType).HasColumnName("EventType").HasMaxLength(256).IsRequired();
        builder.Property(x => x.NotificationTemplateId).HasColumnName("NotificationTemplateId");
        builder.Property(x => x.Channel).HasColumnName("Channel").HasMaxLength(32);
        builder.Property(x => x.Description).HasColumnName("Description").HasMaxLength(1024);
        builder.Property(x => x.IsActive).HasColumnName("IsActive").IsRequired();
        builder.Property(x => x.CreatedAtUtc).HasColumnName("CreatedAtUtc").IsRequired();
        builder.Property(x => x.UpdatedAtUtc).HasColumnName("UpdatedAtUtc").IsRequired();

        builder.HasIndex(x => x.EventType).IsUnique();
        builder.HasIndex(x => x.IsActive);
        builder.HasIndex(x => x.Channel);
        builder.HasIndex(x => x.NotificationTemplateId);
    }
}
