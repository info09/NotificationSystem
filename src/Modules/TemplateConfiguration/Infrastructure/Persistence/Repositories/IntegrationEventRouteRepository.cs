using Microsoft.EntityFrameworkCore;
using TemplateConfiguration.Application.Abstractions;
using TemplateConfiguration.Domain.IntegrationEventRoutes;

namespace TemplateConfiguration.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository EF Core cho Định tuyến sự kiện tích hợp.
/// </summary>
public sealed class IntegrationEventRouteRepository : IIntegrationEventRouteRepository
{
    private readonly TemplateConfigurationDbContext _dbContext;

    /// <summary>
    /// Khởi tạo repository Định tuyến sự kiện tích hợp.
    /// </summary>
    /// <param name="dbContext">DbContext của module TemplateConfiguration.</param>
    public IntegrationEventRouteRepository(TemplateConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<IntegrationEventRoute?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.IntegrationEventRoutes.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<(IReadOnlyList<IntegrationEventRoute> Items, int TotalCount)> GetPagedAsync(
        string? eventType,
        string? channel,
        bool? isActive,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        IQueryable<IntegrationEventRoute> query = _dbContext.IntegrationEventRoutes.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(eventType))
        {
            var normalizedEventType = eventType.Trim();
            query = query.Where(x => x.EventType == normalizedEventType);
        }

        if (!string.IsNullOrWhiteSpace(channel))
        {
            var normalizedChannel = channel.Trim();
            query = query.Where(x => x.Channel == normalizedChannel);
        }

        if (isActive.HasValue)
        {
            query = query.Where(x => x.IsActive == isActive.Value);
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
    public Task<bool> ExistsByEventTypeAsync(string eventType, Guid? excludeId, CancellationToken cancellationToken)
    {
        var normalized = eventType.Trim();
        var query = _dbContext.IntegrationEventRoutes.AsNoTracking().Where(x => x.EventType == normalized);
        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        return query.AnyAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(IntegrationEventRoute route, CancellationToken cancellationToken)
    {
        await _dbContext.IntegrationEventRoutes.AddAsync(route, cancellationToken);
    }

    /// <inheritdoc />
    public void Remove(IntegrationEventRoute route)
    {
        _dbContext.IntegrationEventRoutes.Remove(route);
    }

    /// <inheritdoc />
    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
