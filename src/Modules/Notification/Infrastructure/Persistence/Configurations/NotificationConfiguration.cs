using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DomainNotification = global::Notification.Domain.Notification;
using Recipient = global::Notification.Domain.Recipient;

namespace Notification.Infrastructure.Persistence.Configurations;

public sealed class NotificationConfiguration : IEntityTypeConfiguration<DomainNotification>
{
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
    }
}
