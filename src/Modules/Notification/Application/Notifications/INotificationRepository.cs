using BuildingBlocks.SharedKernel.Repository;
using DomainNotification = global::Notification.Domain.Notification;
using NotificationChannel = global::Notification.Domain.NotificationChannel;
using NotificationStatus = global::Notification.Domain.NotificationStatus;

namespace Notification.Application.Notifications;

public interface INotificationRepository : IRepository<DomainNotification>
{
    Task<(IReadOnlyList<DomainNotification> Items, int TotalCount)> GetPagedAsync(
        NotificationStatus? status,
        NotificationChannel? channel,
        DateTime? createdFromUtc,
        DateTime? createdToUtc,
        int page,
        int pageSize,
        string? scopeRestriction,
        CancellationToken cancellationToken);

    Task<DomainNotification?> GetByIdForScopeAsync(
        Guid id,
        string? scopeRestriction,
        CancellationToken cancellationToken);
}