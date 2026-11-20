using BuildingBlocks.SharedKernel.Repository;
using DomainNotification = Notification.Domain.Notification;

namespace Notification.Application.Notifications.GetNotifications;

public sealed class GetNotificationsUseCase
{
    private readonly IRepository<DomainNotification> _notificationRepository;

    public GetNotificationsUseCase(IRepository<DomainNotification> notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<IReadOnlyList<NotificationDto>> HandleAsync(CancellationToken cancellationToken)
    {
        var notifications = await _notificationRepository.GetAllAsync(cancellationToken);
        return notifications.Select(NotificationMapper.MapToDto).ToList();
    }
}
