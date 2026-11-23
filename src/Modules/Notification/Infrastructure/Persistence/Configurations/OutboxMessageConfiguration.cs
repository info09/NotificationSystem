using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Notification.Infrastructure.Persistence.Configurations;

/// <summary>
/// Cấu hình ánh xạ EF Core cho bản ghi Outbox trong ngữ cảnh Notification.
/// </summary>
internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessageRecord>
{
    /// <summary>
    /// Cấu hình bảng, cột và chỉ mục cho bản ghi Outbox.
    /// </summary>
    /// <param name="builder">Builder cấu hình thực thể.</param>
    public void Configure(EntityTypeBuilder<OutboxMessageRecord> builder)
    {
        builder.ToTable("OutboxMessages", "integration_messaging", t => t.ExcludeFromMigrations());

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("Id")
            .ValueGeneratedNever();

        builder.Property(x => x.MessageId)
            .HasColumnName("MessageId")
            .IsRequired();

        builder.Property(x => x.EventType)
            .HasColumnName("EventType")
            .IsRequired();

        builder.Property(x => x.Payload)
            .HasColumnName("Payload")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("Status")
            .IsRequired();

        builder.Property(x => x.OccurredAtUtc)
            .HasColumnName("OccurredAtUtc")
            .IsRequired();

        builder.Property(x => x.ProcessedAtUtc)
            .HasColumnName("ProcessedAtUtc");

        builder.HasIndex(x => x.MessageId)
            .IsUnique();

        builder.HasIndex(x => x.Status);

        builder.HasIndex(x => x.OccurredAtUtc);
    }
}
