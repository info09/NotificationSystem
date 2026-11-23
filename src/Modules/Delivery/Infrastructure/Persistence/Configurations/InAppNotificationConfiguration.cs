using Delivery.Domain.InAppNotifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Delivery.Infrastructure.Persistence.Configurations;

/// <summary>
/// Cấu hình ánh xạ EF Core cho thực thể Thông báo trong ứng dụng.
/// </summary>
public sealed class InAppNotificationConfiguration : IEntityTypeConfiguration<InAppNotification>
{
    /// <summary>
    /// Cấu hình bảng, cột, chuyển đổi và chỉ mục cho Thông báo trong ứng dụng.
    /// </summary>
    /// <param name="builder">Builder cấu hình thực thể.</param>
    public void Configure(EntityTypeBuilder<InAppNotification> builder)
    {
        builder.ToTable("InAppNotifications");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("Id")
            .ValueGeneratedNever();

        builder.Property(x => x.NotificationId)
            .HasColumnName("NotificationId")
            .IsRequired();

        builder.Property(x => x.RecipientUserId)
            .HasColumnName("RecipientUserId")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.Title)
            .HasColumnName("Title")
            .HasMaxLength(500);

        builder.Property(x => x.Content)
            .HasColumnName("Content");

        builder.Property(x => x.ReadStatus)
            .HasColumnName("ReadStatus")
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .HasColumnName("CreatedAtUtc")
            .IsRequired();

        builder.Property(x => x.ReadAtUtc)
            .HasColumnName("ReadAtUtc");

        builder.HasIndex(x => new { x.NotificationId, x.RecipientUserId })
            .IsUnique();

        builder.HasIndex(x => new { x.RecipientUserId, x.ReadStatus, x.CreatedAtUtc });
        builder.HasIndex(x => new { x.RecipientUserId, x.CreatedAtUtc });
    }
}
