using BuildingBlocks.SharedKernel.Repository;
using Delivery.Application.Abstractions;
using Delivery.Domain.InAppNotifications;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Infrastructure.Persistence;

/// <summary>
/// Repository triển khai truy cập dữ liệu Thông báo trong ứng dụng bằng EF Core.
/// </summary>
public sealed class InAppNotificationRepository : RepositoryBase<InAppNotification>, IInAppNotificationRepository
{
    private readonly DeliveryDbContext _dbContext;

    /// <summary>
    /// Khởi tạo repository với <see cref="DeliveryDbContext"/>.
    /// </summary>
    /// <param name="dbContext">DbContext của module Delivery.</param>
    public InAppNotificationRepository(DeliveryDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<(IReadOnlyList<InAppNotification> Items, int TotalCount)> GetPagedAsync(
        string recipientUserId,
        bool unreadOnly,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        IQueryable<InAppNotification> query = _dbContext.InAppNotifications
            .AsNoTracking()
            .Where(x => x.RecipientUserId == recipientUserId);

        if (unreadOnly)
        {
            query = query.Where(x => x.ReadStatus == ReadStatus.Unread);
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
    public async Task<InAppNotification?> GetByIdForRecipientAsync(
        Guid id,
        string recipientUserId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.InAppNotifications
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.RecipientUserId == recipientUserId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> GetUnreadCountAsync(string recipientUserId, CancellationToken cancellationToken)
    {
        return await _dbContext.InAppNotifications
            .AsNoTracking()
            .CountAsync(x => x.RecipientUserId == recipientUserId && x.ReadStatus == ReadStatus.Unread, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<InAppNotification?> FindByNotificationAndRecipientAsync(
        Guid notificationId,
        string recipientUserId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.InAppNotifications
            .FirstOrDefaultAsync(x => x.NotificationId == notificationId && x.RecipientUserId == recipientUserId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<InAppNotification?> GetTrackedByIdForRecipientAsync(
        Guid id,
        string recipientUserId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.InAppNotifications
            .FirstOrDefaultAsync(x => x.Id == id && x.RecipientUserId == recipientUserId, cancellationToken);
    }
}
