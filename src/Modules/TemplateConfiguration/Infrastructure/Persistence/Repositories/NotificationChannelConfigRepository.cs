using Microsoft.EntityFrameworkCore;
using TemplateConfiguration.Application.Abstractions;
using TemplateConfiguration.Domain.NotificationChannelConfigs;

namespace TemplateConfiguration.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository EF Core cho Cấu hình kênh thông báo.
/// </summary>
public sealed class NotificationChannelConfigRepository : INotificationChannelConfigRepository
{
    private readonly TemplateConfigurationDbContext _dbContext;

    /// <summary>
    /// Khởi tạo repository Cấu hình kênh thông báo.
    /// </summary>
    /// <param name="dbContext">DbContext của module TemplateConfiguration.</param>
    public NotificationChannelConfigRepository(TemplateConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<NotificationChannelConfig?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.NotificationChannelConfigs.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<(IReadOnlyList<NotificationChannelConfig> Items, int TotalCount)> GetPagedAsync(
        string? channel,
        bool? isEnabled,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        IQueryable<NotificationChannelConfig> query = _dbContext.NotificationChannelConfigs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(channel))
        {
            var normalizedChannel = channel.Trim();
            query = query.Where(x => x.Channel == normalizedChannel);
        }

        if (isEnabled.HasValue)
        {
            query = query.Where(x => x.IsEnabled == isEnabled.Value);
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
    public Task<bool> ExistsByChannelAsync(string channel, Guid? excludeId, CancellationToken cancellationToken)
    {
        var normalized = channel.Trim();
        var query = _dbContext.NotificationChannelConfigs.AsNoTracking().Where(x => x.Channel == normalized);
        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        return query.AnyAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(NotificationChannelConfig config, CancellationToken cancellationToken)
    {
        await _dbContext.NotificationChannelConfigs.AddAsync(config, cancellationToken);
    }

    /// <inheritdoc />
    public void Remove(NotificationChannelConfig config)
    {
        _dbContext.NotificationChannelConfigs.Remove(config);
    }

    /// <inheritdoc />
    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
