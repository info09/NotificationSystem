using Microsoft.EntityFrameworkCore;
using TemplateConfiguration.Application.Abstractions;
using TemplateConfiguration.Domain.NotificationTemplates;

namespace TemplateConfiguration.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository EF Core cho Mẫu thông báo.
/// </summary>
public sealed class NotificationTemplateRepository : INotificationTemplateRepository
{
    private readonly TemplateConfigurationDbContext _dbContext;

    /// <summary>
    /// Khởi tạo repository Mẫu thông báo.
    /// </summary>
    /// <param name="dbContext">DbContext của module TemplateConfiguration.</param>
    public NotificationTemplateRepository(TemplateConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<NotificationTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.NotificationTemplates.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<(IReadOnlyList<NotificationTemplate> Items, int TotalCount)> GetPagedAsync(
        string? channel,
        bool? isActive,
        string? keyword,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        IQueryable<NotificationTemplate> query = _dbContext.NotificationTemplates.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(channel))
        {
            var normalizedChannel = channel.Trim();
            query = query.Where(x => x.Channel == normalizedChannel);
        }

        if (isActive.HasValue)
        {
            query = query.Where(x => x.IsActive == isActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var normalizedKeyword = keyword.Trim().ToLower();
            query = query.Where(x =>
                x.TemplateKey.ToLower().Contains(normalizedKeyword) ||
                x.Name.ToLower().Contains(normalizedKeyword) ||
                (x.Description != null && x.Description.ToLower().Contains(normalizedKeyword)));
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
    public Task<bool> ExistsByTemplateKeyAsync(string templateKey, Guid? excludeId, CancellationToken cancellationToken)
    {
        var normalized = templateKey.Trim();
        var query = _dbContext.NotificationTemplates.AsNoTracking().Where(x => x.TemplateKey == normalized);
        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        return query.AnyAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(NotificationTemplate template, CancellationToken cancellationToken)
    {
        await _dbContext.NotificationTemplates.AddAsync(template, cancellationToken);
    }

    /// <inheritdoc />
    public void Remove(NotificationTemplate template)
    {
        _dbContext.NotificationTemplates.Remove(template);
    }

    /// <inheritdoc />
    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
