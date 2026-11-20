using BuildingBlocks.SharedKernel.Repository;
using DomainNotification = Notification.Domain.Notification;

namespace Notification.Application.Notifications.GetNotificationById;

public sealed class GetNotificationByIdUseCase
{
    private readonly IRepository<DomainNotification> _notificationRepository;

    public GetNotificationByIdUseCase(IRepository<DomainNotification> notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<NotificationDto?> HandleAsync(Guid id, CancellationToken cancellationToken)
    {
        var notification = await _notificationRepository.GetByIdAsync(id, cancellationToken);
        return notification is null ? null : NotificationMapper.MapToDto(notification);
    }
}
