using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DomainNotification = global::Notification.Domain.Notification;
using Recipient = global::Notification.Domain.Recipient;

namespace Notification.Infrastructure.Persistence.Configurations;

/// <summary>
/// Cấu hình ánh xạ EF Core cho thực thể Thông báo.
/// </summary>
public sealed class NotificationConfiguration : IEntityTypeConfiguration<DomainNotification>
{
    /// <summary>
    /// Cấu hình bảng, cột, chuyển đổi và chỉ mục cho thực thể Thông báo.
    /// </summary>
    /// <param name="builder">Builder cấu hình thực thể.</param>
    public void Configure(EntityTypeBuilder<DomainNotification> builder)
    {
        builder.ToTable("Notifications");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("Id")
            .ValueGeneratedNever();

        builder.Property(x => x.Recipient)
            .HasColumnName("Recipient")
            .HasConversion(
                recipient => recipient.Value,
                value => new Recipient(value))
            .IsRequired();

        builder.Property(x => x.Channel)
            .HasColumnName("Channel")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("Status")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.Content)
            .HasColumnName("Content");

        builder.Property(x => x.NotificationTemplateId)
            .HasColumnName("NotificationTemplateId");

        builder.Property(x => x.IdempotencyScope)
            .HasColumnName("IdempotencyScope")
            .IsRequired();

        builder.Property(x => x.IdempotencyKey)
            .HasColumnName("IdempotencyKey")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("CreatedAt")
            .IsRequired();

        builder.HasIndex(x => new { x.IdempotencyScope, x.IdempotencyKey })
            .IsUnique();

        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.Channel);
        builder.HasIndex(x => x.CreatedAt);
    }
}
