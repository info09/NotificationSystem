using IdentityAccess.Application.Abstractions;
using IdentityAccess.Domain.IntegrationClientCredentials;
using Microsoft.EntityFrameworkCore;

namespace IdentityAccess.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository EF Core cho Thông tin xác thực IntegrationClient.
/// </summary>
public sealed class IntegrationClientCredentialRepository : IIntegrationClientCredentialRepository
{
    private readonly IdentityAccessDbContext _dbContext;

    /// <summary>
    /// Khởi tạo repository Thông tin xác thực IntegrationClient.
    /// </summary>
    /// <param name="dbContext">DbContext của module IdentityAccess.</param>
    public IntegrationClientCredentialRepository(IdentityAccessDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<IntegrationClientCredential?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.IntegrationClientCredentials.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<IntegrationClientCredential>> GetByUserAccountIdAsync(Guid userAccountId, CancellationToken cancellationToken)
    {
        return await _dbContext.IntegrationClientCredentials
            .AsNoTracking()
            .Where(x => x.UserAccountId == userAccountId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task<bool> ExistsByClientIdAsync(string clientId, Guid? excludeId, CancellationToken cancellationToken)
    {
        var normalized = clientId.Trim();
        var query = _dbContext.IntegrationClientCredentials.AsNoTracking().Where(x => x.ClientId == normalized);
        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        return query.AnyAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(IntegrationClientCredential credential, CancellationToken cancellationToken)
    {
        await _dbContext.IntegrationClientCredentials.AddAsync(credential, cancellationToken);
    }

    /// <inheritdoc />
    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
