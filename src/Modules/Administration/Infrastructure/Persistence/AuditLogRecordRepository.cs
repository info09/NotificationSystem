using Administration.Application.Abstractions;
using Administration.Domain;
using Microsoft.EntityFrameworkCore;

namespace Administration.Infrastructure.Persistence;

/// <summary>
/// Repository EF Core cho Nhật ký kiểm toán.
/// </summary>
public sealed class AuditLogRecordRepository : IAuditLogRecordRepository
{
    private readonly AdministrationDbContext _dbContext;

    /// <summary>
    /// Khởi tạo repository Nhật ký kiểm toán.
    /// </summary>
    /// <param name="dbContext">DbContext của module Administration.</param>
    public AuditLogRecordRepository(AdministrationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<(IReadOnlyList<AuditLogRecord> Items, int TotalCount)> GetPagedAsync(
        string? action,
        string? targetType,
        string? actorUserId,
        DateTime? fromUtc,
        DateTime? toUtc,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        IQueryable<AuditLogRecord> query = _dbContext.AuditLogRecords.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(action))
        {
            var normalizedAction = action.Trim();
            query = query.Where(x => x.Action == normalizedAction);
        }

        if (!string.IsNullOrWhiteSpace(targetType))
        {
            var normalizedTargetType = targetType.Trim();
            query = query.Where(x => x.TargetType == normalizedTargetType);
        }

        if (!string.IsNullOrWhiteSpace(actorUserId))
        {
            var normalizedActorUserId = actorUserId.Trim();
            query = query.Where(x => x.ActorUserId == normalizedActorUserId);
        }

        if (fromUtc.HasValue)
        {
            query = query.Where(x => x.OccurredAtUtc >= fromUtc.Value);
        }

        if (toUtc.HasValue)
        {
            query = query.Where(x => x.OccurredAtUtc <= toUtc.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.OccurredAtUtc)
            .ThenByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
