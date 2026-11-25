using IdentityAccess.Application.Abstractions;
using IdentityAccess.Domain.PasswordResetTokens;
using Microsoft.EntityFrameworkCore;

namespace IdentityAccess.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository EF Core cho Mã đặt lại mật khẩu.
/// </summary>
public sealed class PasswordResetTokenRepository : IPasswordResetTokenRepository
{
    private readonly IdentityAccessDbContext _dbContext;

    /// <summary>
    /// Khởi tạo repository Mã đặt lại mật khẩu.
    /// </summary>
    /// <param name="dbContext">DbContext của module IdentityAccess.</param>
    public PasswordResetTokenRepository(IdentityAccessDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task AddAsync(PasswordResetToken token, CancellationToken cancellationToken)
    {
        await _dbContext.PasswordResetTokens.AddAsync(token, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PasswordResetToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken)
    {
        return await _dbContext.PasswordResetTokens.FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PasswordResetToken>> GetActiveByUserAccountIdAsync(Guid userAccountId, DateTime utcNow, CancellationToken cancellationToken)
    {
        return await _dbContext.PasswordResetTokens
            .Where(x => x.UserAccountId == userAccountId && x.UsedAtUtc == null && x.ExpiresAtUtc > utcNow)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
