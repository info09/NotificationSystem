using Administration.Domain;
using BuildingBlocks.SharedKernel.Auditing;

namespace Administration.Infrastructure.Persistence;

/// <summary>
/// Bộ ghi Nhật ký kiểm toán dùng chung qua SharedKernel.
/// </summary>
public sealed class AuditLogRecorder : IAuditLogRecorder
{
    private readonly AdministrationDbContext _dbContext;

    /// <summary>
    /// Khởi tạo bộ ghi Nhật ký kiểm toán.
    /// </summary>
    /// <param name="dbContext">DbContext của module Administration.</param>
    public AuditLogRecorder(AdministrationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task RecordAsync(AuditLogEntry entry, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entry);

        var record = AuditLogRecord.Create(
            entry.ActorUserId,
            entry.ActorRole ?? string.Empty,
            entry.Action,
            entry.TargetType,
            entry.TargetId,
            entry.Result,
            entry.Reason,
            entry.CorrelationId,
            entry.MetadataJson);

        _dbContext.AuditLogRecords.Add(record);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
