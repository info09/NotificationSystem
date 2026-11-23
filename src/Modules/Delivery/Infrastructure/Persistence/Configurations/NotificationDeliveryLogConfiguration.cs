using Delivery.Domain.DeliveryLogs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Delivery.Infrastructure.Persistence.Configurations;

/// <summary>
/// Cấu hình ánh xạ EF Core cho thực thể Nhật ký gửi thông báo.
/// </summary>
public sealed class NotificationDeliveryLogConfiguration : IEntityTypeConfiguration<NotificationDeliveryLog>
{
    /// <summary>
    /// Cấu hình bảng, cột, chuyển đổi và chỉ mục cho Nhật ký gửi thông báo.
    /// </summary>
    /// <param name="builder">Builder cấu hình thực thể.</param>
    public void Configure(EntityTypeBuilder<NotificationDeliveryLog> builder)
    {
        builder.ToTable("NotificationDeliveryLogs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("Id")
            .ValueGeneratedNever();

        builder.Property(x => x.NotificationId)
            .HasColumnName("NotificationId")
            .IsRequired();

        builder.Property(x => x.Channel)
            .HasColumnName("Channel")
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.Result)
            .HasColumnName("Result")
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.ErrorMessage)
            .HasColumnName("ErrorMessage");

        builder.Property(x => x.RetryCount)
            .HasColumnName("RetryCount")
            .IsRequired();

        builder.Property(x => x.CorrelationId)
            .HasColumnName("CorrelationId")
            .HasMaxLength(256);

        builder.Property(x => x.CreatedAtUtc)
            .HasColumnName("CreatedAtUtc")
            .IsRequired();

        builder.HasIndex(x => x.NotificationId);
        builder.HasIndex(x => new { x.NotificationId, x.CreatedAtUtc });
        builder.HasIndex(x => x.CorrelationId);
    }
}
