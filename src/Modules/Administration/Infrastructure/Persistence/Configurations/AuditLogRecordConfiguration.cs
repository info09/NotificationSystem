using Administration.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Administration.Infrastructure.Persistence.Configurations;

/// <summary>
/// Cấu hình ánh xạ EF Core cho bản ghi Nhật ký kiểm toán.
/// </summary>
public sealed class AuditLogRecordConfiguration : IEntityTypeConfiguration<AuditLogRecord>
{
    /// <summary>
    /// Cấu hình bảng và thuộc tính cho bản ghi Nhật ký kiểm toán.
    /// </summary>
    /// <param name="builder">Builder cấu hình thực thể.</param>
    public void Configure(EntityTypeBuilder<AuditLogRecord> builder)
    {
        builder.ToTable("AuditLogRecords", "administration");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("Id");
        builder.Property(x => x.ActorUserId).HasColumnName("ActorUserId").IsRequired();
        builder.Property(x => x.ActorRole).HasColumnName("ActorRole").IsRequired();
        builder.Property(x => x.Action).HasColumnName("Action").IsRequired();
        builder.Property(x => x.TargetType).HasColumnName("TargetType").IsRequired();
        builder.Property(x => x.TargetId).HasColumnName("TargetId").IsRequired();
        builder.Property(x => x.Result).HasColumnName("Result").IsRequired();
        builder.Property(x => x.Reason).HasColumnName("Reason");
        builder.Property(x => x.CorrelationId).HasColumnName("CorrelationId");
        builder.Property(x => x.MetadataJson).HasColumnName("MetadataJson").HasColumnType("text");
        builder.Property(x => x.OccurredAtUtc).HasColumnName("OccurredAtUtc").HasColumnType("timestamp with time zone");

        builder.HasIndex(x => x.OccurredAtUtc);
        builder.HasIndex(x => new { x.TargetType, x.TargetId });
    }
}
