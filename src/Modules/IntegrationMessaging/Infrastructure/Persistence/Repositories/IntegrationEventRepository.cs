using IntegrationMessaging.Application.Abstractions;
using IntegrationMessaging.Domain;
using Microsoft.EntityFrameworkCore;

namespace IntegrationMessaging.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository EF Core cho Sự kiện tích hợp.
/// </summary>
public sealed class IntegrationEventRepository : IIntegrationEventRepository
{
    private readonly IntegrationMessagingDbContext _dbContext;

    /// <summary>
    /// Khởi tạo repository Sự kiện tích hợp.
    /// </summary>
    /// <param name="dbContext">DbContext của module IntegrationMessaging.</param>
    public IntegrationEventRepository(IntegrationMessagingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<IntegrationEvent?> GetByMessageIdAsync(Guid messageId, CancellationToken cancellationToken)
    {
        return await _dbContext.IntegrationEvents
            .FirstOrDefaultAsync(x => x.MessageId == messageId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(IntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        await _dbContext.IntegrationEvents.AddAsync(integrationEvent, cancellationToken);
    }

    /// <inheritdoc />
    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
