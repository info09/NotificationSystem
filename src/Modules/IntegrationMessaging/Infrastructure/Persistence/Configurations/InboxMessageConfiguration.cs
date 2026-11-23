using IntegrationMessaging.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IntegrationMessaging.Infrastructure.Persistence.Configurations;

/// <summary>
/// Cấu hình ánh xạ EF Core cho thực thể InboxMessage.
/// </summary>
public sealed class InboxMessageConfiguration : IEntityTypeConfiguration<InboxMessage>
{
    /// <summary>
    /// Cấu hình bảng, cột và chỉ mục cho thực thể InboxMessage.
    /// </summary>
    /// <param name="builder">Builder cấu hình thực thể.</param>
    public void Configure(EntityTypeBuilder<InboxMessage> builder)
    {
        builder.ToTable("InboxMessages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("Id")
            .ValueGeneratedNever();

        builder.Property(x => x.MessageId)
            .HasColumnName("MessageId")
            .IsRequired();

        builder.Property(x => x.ConsumerName)
            .HasColumnName("ConsumerName")
            .IsRequired();

        builder.Property(x => x.EventType)
            .HasColumnName("EventType")
            .IsRequired();

        builder.Property(x => x.ProcessedAtUtc)
            .HasColumnName("ProcessedAtUtc")
            .IsRequired();

        builder.HasIndex(x => new { x.MessageId, x.ConsumerName })
            .IsUnique();
    }
}
