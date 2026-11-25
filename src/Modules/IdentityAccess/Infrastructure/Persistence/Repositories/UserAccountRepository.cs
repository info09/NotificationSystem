using IdentityAccess.Application.Abstractions;
using IdentityAccess.Domain.UserAccounts;
using Microsoft.EntityFrameworkCore;

namespace IdentityAccess.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository EF Core cho Tài khoản người dùng.
/// </summary>
public sealed class UserAccountRepository : IUserAccountRepository
{
    private readonly IdentityAccessDbContext _dbContext;

    /// <summary>
    /// Khởi tạo repository Tài khoản người dùng.
    /// </summary>
    /// <param name="dbContext">DbContext của module IdentityAccess.</param>
    public UserAccountRepository(IdentityAccessDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<UserAccount?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.UserAccounts.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<UserAccount?> GetByUserNameAsync(string userName, CancellationToken cancellationToken)
    {
        var normalized = userName.Trim();
        return await _dbContext.UserAccounts.FirstOrDefaultAsync(x => x.UserName == normalized, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<(IReadOnlyList<UserAccount> Items, int TotalCount)> GetPagedAsync(
        UserAccountStatus? status,
        UserRole? role,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        IQueryable<UserAccount> query = _dbContext.UserAccounts.AsNoTracking();

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }
        else
        {
            query = query.Where(x => x.Status != UserAccountStatus.Deleted);
        }

        if (role.HasValue)
        {
            query = query.Where(x => x.Role == role.Value);
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
    public Task<bool> ExistsByUserNameAsync(string userName, Guid? excludeId, CancellationToken cancellationToken)
    {
        var normalized = userName.Trim();
        var query = _dbContext.UserAccounts.AsNoTracking().Where(x => x.UserName == normalized);
        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        return query.AnyAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task<bool> ExistsByEmailAsync(string email, Guid? excludeId, CancellationToken cancellationToken)
    {
        var normalized = email.Trim();
        var query = _dbContext.UserAccounts.AsNoTracking().Where(x => x.Email == normalized);
        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        return query.AnyAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(UserAccount account, CancellationToken cancellationToken)
    {
        await _dbContext.UserAccounts.AddAsync(account, cancellationToken);
    }

    /// <inheritdoc />
    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
