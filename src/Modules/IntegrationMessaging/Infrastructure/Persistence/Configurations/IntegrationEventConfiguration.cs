using IntegrationMessaging.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IntegrationMessaging.Infrastructure.Persistence.Configurations;

/// <summary>
/// Cấu hình ánh xạ EF Core cho thực thể Sự kiện tích hợp.
/// </summary>
public sealed class IntegrationEventConfiguration : IEntityTypeConfiguration<IntegrationEvent>
{
    /// <summary>
    /// Cấu hình bảng, cột và chỉ mục cho thực thể Sự kiện tích hợp.
    /// </summary>
    /// <param name="builder">Builder cấu hình thực thể.</param>
    public void Configure(EntityTypeBuilder<IntegrationEvent> builder)
    {
        builder.ToTable("IntegrationEvents");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("Id").ValueGeneratedNever();
        builder.Property(x => x.MessageId).HasColumnName("MessageId").IsRequired();
        builder.Property(x => x.EventId).HasColumnName("EventId").IsRequired();
        builder.Property(x => x.EventType).HasColumnName("EventType").HasMaxLength(256).IsRequired();
        builder.Property(x => x.OccurredAtUtc).HasColumnName("OccurredAtUtc").IsRequired();
        builder.Property(x => x.Source).HasColumnName("Source").HasMaxLength(256).IsRequired();
        builder.Property(x => x.IdempotencyKey).HasColumnName("IdempotencyKey").HasMaxLength(256);
        builder.Property(x => x.Version).HasColumnName("Version").HasMaxLength(64).IsRequired();
        builder.Property(x => x.Payload).HasColumnName("Payload").IsRequired();
        builder.Property(x => x.ReceivedAtUtc).HasColumnName("ReceivedAtUtc").IsRequired();

        builder.HasIndex(x => x.MessageId).IsUnique();
        builder.HasIndex(x => x.EventId);
        builder.HasIndex(x => x.EventType);
        builder.HasIndex(x => x.ReceivedAtUtc);
    }
}
