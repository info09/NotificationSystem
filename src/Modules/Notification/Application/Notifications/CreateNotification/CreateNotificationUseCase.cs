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

    public async Task<CreateNotificationResult> HandleAsync(CreateNotificationCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.IdempotencyScope))
        {
            throw new ArgumentException("Idempotency scope is required.", nameof(command));
        }

        if (string.IsNullOrWhiteSpace(command.IdempotencyKey))
        {
            throw new ArgumentException("Idempotency key is required.", nameof(command));
        }

        var existingNotifications = await _notificationRepository.FindAsync(
            notification => notification.IdempotencyScope == command.IdempotencyScope
                && notification.IdempotencyKey == command.IdempotencyKey,
            cancellationToken);

        var existingNotification = existingNotifications.FirstOrDefault();
        if (existingNotification is not null)
        {
            return new CreateNotificationResult(NotificationMapper.MapToDto(existingNotification), Created: false);
        }

        var notification = DomainNotification.Create(
            Guid.NewGuid(),
            new Recipient(command.Recipient),
            command.Channel,
            command.Content,
            command.NotificationTemplateId,
            command.IdempotencyScope,
            command.IdempotencyKey);

        try
        {
            await _notificationRepository.AddAsync(notification, cancellationToken);
            await _notificationRepository.SaveChangesAsync(cancellationToken);

            return new CreateNotificationResult(NotificationMapper.MapToDto(notification), Created: true);
        }
        catch
        {
            var concurrentNotifications = await _notificationRepository.FindAsync(
                n => n.IdempotencyScope == command.IdempotencyScope
                    && n.IdempotencyKey == command.IdempotencyKey,
                cancellationToken);

            var concurrentNotification = concurrentNotifications.FirstOrDefault();
            if (concurrentNotification is not null)
            {
                return new CreateNotificationResult(NotificationMapper.MapToDto(concurrentNotification), Created: false);
            }

            throw;
        }
    }
}
