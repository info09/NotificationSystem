using BuildingBlocks.SharedKernel.Repository;
using DomainNotification = Notification.Domain.Notification;
using Recipient = Notification.Domain.Recipient;

namespace Notification.Application.Notifications.CreateNotification;

public sealed class CreateNotificationUseCase
{
    private readonly IRepository<DomainNotification> _notificationRepository;

    public CreateNotificationUseCase(IRepository<DomainNotification> notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<NotificationDto> HandleAsync(CreateNotificationCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var notification = DomainNotification.Create(
            Guid.NewGuid(),
            new Recipient(command.Recipient),
            command.Channel,
            command.Content,
            command.NotificationTemplateId);

        await _notificationRepository.AddAsync(notification, cancellationToken);

        return NotificationMapper.MapToDto(notification);
    }
}
