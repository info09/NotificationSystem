using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TemplateConfiguration.Domain.NotificationTemplates;

namespace TemplateConfiguration.Infrastructure.Persistence.Configurations;

/// <summary>
/// Cấu hình ánh xạ EF Core cho thực thể Mẫu thông báo.
/// </summary>
public sealed class NotificationTemplateConfiguration : IEntityTypeConfiguration<NotificationTemplate>
{
    /// <summary>
    /// Cấu hình bảng, cột và chỉ mục cho Mẫu thông báo.
    /// </summary>
    /// <param name="builder">Builder cấu hình thực thể.</param>
    public void Configure(EntityTypeBuilder<NotificationTemplate> builder)
    {
        builder.ToTable("NotificationTemplates");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("Id").ValueGeneratedNever();
        builder.Property(x => x.TemplateKey).HasColumnName("TemplateKey").HasMaxLength(128).IsRequired();
        builder.Property(x => x.Name).HasColumnName("Name").HasMaxLength(256).IsRequired();
        builder.Property(x => x.Description).HasColumnName("Description").HasMaxLength(1024);
        builder.Property(x => x.Channel).HasColumnName("Channel").HasMaxLength(32).IsRequired();
        builder.Property(x => x.Subject).HasColumnName("Subject").HasMaxLength(512);
        builder.Property(x => x.Body).HasColumnName("Body").IsRequired();
        builder.Property(x => x.IsActive).HasColumnName("IsActive").IsRequired();
        builder.Property(x => x.CreatedAtUtc).HasColumnName("CreatedAtUtc").IsRequired();
        builder.Property(x => x.UpdatedAtUtc).HasColumnName("UpdatedAtUtc").IsRequired();

        builder.HasIndex(x => x.TemplateKey).IsUnique();
        builder.HasIndex(x => x.Channel);
        builder.HasIndex(x => x.IsActive);
    }
}
