using BuildingBlocks.SharedKernel.Repository;
using Microsoft.EntityFrameworkCore;
using Notification.Application.Notifications;
using DomainNotification = global::Notification.Domain.Notification;
using NotificationChannel = global::Notification.Domain.NotificationChannel;
using NotificationStatus = global::Notification.Domain.NotificationStatus;

namespace Notification.Infrastructure.Persistence;

/// <summary>
/// Repository triển khai truy cập dữ liệu Thông báo bằng EF Core.
/// </summary>
public sealed class NotificationRepository : RepositoryBase<DomainNotification>, INotificationRepository
{
    private readonly NotificationDbContext _dbContext;

    /// <summary>
    /// Khởi tạo repository với <see cref="NotificationDbContext"/>.
    /// </summary>
    /// <param name="dbContext">DbContext của module Notification.</param>
    public NotificationRepository(NotificationDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<(IReadOnlyList<DomainNotification> Items, int TotalCount)> GetPagedAsync(
        NotificationStatus? status,
        NotificationChannel? channel,
        DateTime? createdFromUtc,
        DateTime? createdToUtc,
        int page,
        int pageSize,
        string? scopeRestriction,
        CancellationToken cancellationToken)
    {
        IQueryable<DomainNotification> query = _dbContext.Notifications.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(scopeRestriction))
        {
            query = query.Where(n => n.IdempotencyScope == scopeRestriction);
        }

        if (status is not null)
        {
            query = query.Where(n => n.Status == status.Value);
        }

        if (channel is not null)
        {
            query = query.Where(n => n.Channel == channel.Value);
        }

        if (createdFromUtc is not null)
        {
            query = query.Where(n => n.CreatedAt >= createdFromUtc.Value);
        }

        if (createdToUtc is not null)
        {
            query = query.Where(n => n.CreatedAt <= createdToUtc.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(n => n.CreatedAt)
            .ThenByDescending(n => n.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    /// <inheritdoc />
    public async Task<DomainNotification?> GetByIdForScopeAsync(
        Guid id,
        string? scopeRestriction,
        CancellationToken cancellationToken)
    {
        IQueryable<DomainNotification> query = _dbContext.Notifications.AsNoTracking();

        query = query.Where(n => n.Id == id);

        if (!string.IsNullOrWhiteSpace(scopeRestriction))
        {
            query = query.Where(n => n.IdempotencyScope == scopeRestriction);
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }
}
