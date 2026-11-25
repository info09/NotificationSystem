using IdentityAccess.Application.Abstractions;
using IdentityAccess.Domain.EndUserProfiles;
using Microsoft.EntityFrameworkCore;

namespace IdentityAccess.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository EF Core cho Hồ sơ người nhận EndUser.
/// </summary>
public sealed class EndUserProfileRepository : IEndUserProfileRepository
{
    private readonly IdentityAccessDbContext _dbContext;

    /// <summary>
    /// Khởi tạo repository Hồ sơ người nhận EndUser.
    /// </summary>
    /// <param name="dbContext">DbContext của module IdentityAccess.</param>
    public EndUserProfileRepository(IdentityAccessDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<EndUserProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.EndUserProfiles.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<EndUserProfile?> GetByExternalUserIdAsync(string externalUserId, CancellationToken cancellationToken)
    {
        var normalized = externalUserId.Trim();
        return await _dbContext.EndUserProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ExternalUserId == normalized, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<(IReadOnlyList<EndUserProfile> Items, int TotalCount)> GetPagedAsync(
        EndUserProfileStatus? status,
        string? externalUserId,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        IQueryable<EndUserProfile> query = _dbContext.EndUserProfiles.AsNoTracking();

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(externalUserId))
        {
            var normalizedExternalUserId = externalUserId.Trim();
            query = query.Where(x => x.ExternalUserId == normalizedExternalUserId);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .ThenByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    /// <inheritdoc />
    public Task<bool> ExistsByExternalUserIdAsync(string externalUserId, Guid? excludeId, CancellationToken cancellationToken)
    {
        var normalized = externalUserId.Trim();
        var query = _dbContext.EndUserProfiles.AsNoTracking().Where(x => x.ExternalUserId == normalized);
        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        return query.AnyAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(EndUserProfile profile, CancellationToken cancellationToken)
    {
        await _dbContext.EndUserProfiles.AddAsync(profile, cancellationToken);
    }

    /// <inheritdoc />
    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
