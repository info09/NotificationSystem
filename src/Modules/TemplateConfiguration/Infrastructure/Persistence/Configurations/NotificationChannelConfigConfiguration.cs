using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TemplateConfiguration.Domain.NotificationChannelConfigs;

namespace TemplateConfiguration.Infrastructure.Persistence.Configurations;

/// <summary>
/// Cấu hình ánh xạ EF Core cho thực thể Cấu hình kênh thông báo.
/// </summary>
public sealed class NotificationChannelConfigConfiguration : IEntityTypeConfiguration<NotificationChannelConfig>
{
    /// <summary>
    /// Cấu hình bảng, cột và chỉ mục cho Cấu hình kênh thông báo.
    /// </summary>
    /// <param name="builder">Builder cấu hình thực thể.</param>
    public void Configure(EntityTypeBuilder<NotificationChannelConfig> builder)
    {
        builder.ToTable("NotificationChannelConfigs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("Id").ValueGeneratedNever();
        builder.Property(x => x.Channel).HasColumnName("Channel").HasMaxLength(32).IsRequired();
        builder.Property(x => x.DisplayName).HasColumnName("DisplayName").HasMaxLength(256).IsRequired();
        builder.Property(x => x.Description).HasColumnName("Description").HasMaxLength(1024);
        builder.Property(x => x.IsEnabled).HasColumnName("IsEnabled").IsRequired();
        builder.Property(x => x.CreatedAtUtc).HasColumnName("CreatedAtUtc").IsRequired();
        builder.Property(x => x.UpdatedAtUtc).HasColumnName("UpdatedAtUtc").IsRequired();

        builder.HasIndex(x => x.Channel).IsUnique();
        builder.HasIndex(x => x.IsEnabled);
    }
}
