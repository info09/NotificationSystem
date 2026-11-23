using Delivery.Domain.DeliveryLogs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Delivery.Infrastructure.Persistence.Configurations;

/// <summary>
/// Cấu hình ánh xạ EF Core cho thực thể Lần thử gửi.
/// </summary>
public sealed class DeliveryAttemptConfiguration : IEntityTypeConfiguration<DeliveryAttempt>
{
    /// <summary>
    /// Cấu hình bảng, cột, chuyển đổi và chỉ mục cho Lần thử gửi.
    /// </summary>
    /// <param name="builder">Builder cấu hình thực thể.</param>
    public void Configure(EntityTypeBuilder<DeliveryAttempt> builder)
    {
        builder.ToTable("DeliveryAttempts");

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

        builder.Property(x => x.AttemptNumber)
            .HasColumnName("AttemptNumber")
            .IsRequired();

        builder.Property(x => x.Result)
            .HasColumnName("Result")
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.ErrorMessage)
            .HasColumnName("ErrorMessage");

        builder.Property(x => x.CorrelationId)
            .HasColumnName("CorrelationId")
            .HasMaxLength(256);

        builder.Property(x => x.AttemptedAtUtc)
            .HasColumnName("AttemptedAtUtc")
            .IsRequired();

        builder.HasIndex(x => x.NotificationId);
        builder.HasIndex(x => new { x.NotificationId, x.AttemptedAtUtc });
        builder.HasIndex(x => x.CorrelationId);
    }
}
