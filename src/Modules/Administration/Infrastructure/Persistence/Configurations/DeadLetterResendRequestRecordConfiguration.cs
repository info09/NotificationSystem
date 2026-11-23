using Administration.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Administration.Infrastructure.Persistence.Configurations;

/// <summary>
/// Cấu hình ánh xạ EF Core cho bản ghi yêu cầu Gửi lại từ Hàng đợi thư chết.
/// </summary>
public sealed class DeadLetterResendRequestRecordConfiguration : IEntityTypeConfiguration<DeadLetterResendRequestRecord>
{
    /// <summary>
    /// Cấu hình bảng và thuộc tính cho bản ghi yêu cầu Gửi lại.
    /// </summary>
    /// <param name="builder">Builder cấu hình thực thể.</param>
    public void Configure(EntityTypeBuilder<DeadLetterResendRequestRecord> builder)
    {
        builder.ToTable("DeadLetterResendRequestRecords", "administration");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("Id");
        builder.Property(x => x.IdempotencyKey).HasColumnName("IdempotencyKey").IsRequired();
        builder.Property(x => x.ActorUserId).HasColumnName("ActorUserId").IsRequired();
        builder.Property(x => x.QueueName).HasColumnName("QueueName").IsRequired();
        builder.Property(x => x.MessageId).HasColumnName("MessageId").IsRequired();
        builder.Property(x => x.Status).HasColumnName("Status").IsRequired();
        builder.Property(x => x.CreatedAtUtc).HasColumnName("CreatedAtUtc").HasColumnType("timestamp with time zone");
        builder.Property(x => x.CompletedAtUtc).HasColumnName("CompletedAtUtc").HasColumnType("timestamp with time zone");
        builder.Property(x => x.ResultMessageId).HasColumnName("ResultMessageId");

        builder.HasIndex(x => x.IdempotencyKey).IsUnique();
    }
}
