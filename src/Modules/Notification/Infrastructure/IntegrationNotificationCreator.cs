using BuildingBlocks.SharedKernel.IntegrationEvents;
using Notification.Application.Notifications;
using DomainNotification = Notification.Domain.Notification;
using NotificationChannel = Notification.Domain.NotificationChannel;
using Recipient = Notification.Domain.Recipient;

namespace Notification.Infrastructure;

/// <summary>
/// Bộ tạo Thông báo từ Sự kiện tích hợp.
/// </summary>
public sealed class IntegrationNotificationCreator : IIntegrationNotificationCreator
{
    private readonly INotificationRepository _notificationRepository;
    private readonly INotificationOutboxWriter? _outboxWriter;

    /// <summary>
    /// Khởi tạo bộ tạo Thông báo từ Sự kiện tích hợp.
    /// </summary>
    /// <param name="notificationRepository">Repository Thông báo.</param>
    /// <param name="outboxWriter">Port ghi bản ghi Outbox cho Thông báo.</param>
    public IntegrationNotificationCreator(
        INotificationRepository notificationRepository,
        INotificationOutboxWriter? outboxWriter = null)
    {
        _notificationRepository = notificationRepository;
        _outboxWriter = outboxWriter;
    }

    /// <inheritdoc />
    public async Task<IntegrationNotificationCreationResult> CreateAsync(
        CreateIntegrationNotificationCommand command,
        CancellationToken cancellationToken)
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

        if (string.IsNullOrWhiteSpace(command.Recipient))
        {
            throw new ArgumentException("Recipient is required.", nameof(command));
        }

        if (!TryParseChannel(command.Channel, out var channel))
        {
            throw new ArgumentException("Channel must be 'email' or 'inapp'.", nameof(command));
        }

        var existingNotifications = await _notificationRepository.FindAsync(
            notification => notification.IdempotencyScope == command.IdempotencyScope
                && notification.IdempotencyKey == command.IdempotencyKey,
            cancellationToken);

        var existingNotification = existingNotifications.FirstOrDefault();
        if (existingNotification is not null)
        {
            return new IntegrationNotificationCreationResult(
                existingNotification.Id,
                existingNotification.Status.ToString(),
                false);
        }

        var notification = DomainNotification.Create(
            Guid.NewGuid(),
            new Recipient(command.Recipient),
            channel,
            command.Content,
            command.NotificationTemplateId,
            command.IdempotencyScope,
            command.IdempotencyKey);

        try
        {
            await _notificationRepository.AddAsync(notification, cancellationToken);
            _outboxWriter?.AddNotificationCreated(notification);
            await _notificationRepository.SaveChangesAsync(cancellationToken);

            return new IntegrationNotificationCreationResult(notification.Id, notification.Status.ToString(), true);
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
                return new IntegrationNotificationCreationResult(
                    concurrentNotification.Id,
                    concurrentNotification.Status.ToString(),
                    false);
            }

            throw;
        }
    }

    private static bool TryParseChannel(string? channel, out NotificationChannel parsedChannel)
    {
        if (string.Equals(channel, "email", StringComparison.OrdinalIgnoreCase))
        {
            parsedChannel = NotificationChannel.Email;
            return true;
        }

        if (string.Equals(channel, "inapp", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(channel, "in-app", StringComparison.OrdinalIgnoreCase))
        {
            parsedChannel = NotificationChannel.InApp;
            return true;
        }

        parsedChannel = default;
        return false;
    }
}
