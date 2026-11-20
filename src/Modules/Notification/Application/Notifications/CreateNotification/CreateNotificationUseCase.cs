using DomainNotification = global::Notification.Domain.Notification;
using NotificationChannel = global::Notification.Domain.NotificationChannel;
using Recipient = global::Notification.Domain.Recipient;

namespace Notification.Application.Notifications.CreateNotification;

public sealed class CreateNotificationUseCase
{
    private readonly INotificationRepository _notificationRepository;

    public CreateNotificationUseCase(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<NotificationAppResult<CreateNotificationResponseDto>> HandleAsync(
        CreateNotificationAppRequest? request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return NotificationAppResult<CreateNotificationResponseDto>.Validation(
                new Dictionary<string, string[]>
                {
                    ["request"] = ["Request body is required."]
                });
        }

        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.IdempotencyKey))
        {
            errors["idempotencyKey"] = ["IdempotencyKey is required."];
        }

        if (string.IsNullOrWhiteSpace(request.Recipient))
        {
            errors["recipient"] = ["Recipient is required."];
        }

        NotificationChannel parsedChannel = default;
        if (string.IsNullOrWhiteSpace(request.Channel))
        {
            errors["channel"] = ["Channel is required."];
        }
        else if (!NotificationParsing.TryParseChannel(request.Channel, out parsedChannel))
        {
            errors["channel"] = ["Channel must be 'email' or 'inapp'."];
        }

        if (string.IsNullOrWhiteSpace(request.Content) && request.NotificationTemplateId is null)
        {
            errors["content"] = ["Content or notificationTemplateId is required."];
        }

        if (errors.Count > 0)
        {
            return NotificationAppResult<CreateNotificationResponseDto>.Validation(errors);
        }

        var idempotencyScope = request.Caller?.Subject;
        if (string.IsNullOrWhiteSpace(idempotencyScope))
        {
            return NotificationAppResult<CreateNotificationResponseDto>.Validation(
                new Dictionary<string, string[]>
                {
                    ["idempotencyScope"] = ["Authenticated caller scope could not be determined."]
                });
        }

        var command = new CreateNotificationCommand(
            request.Recipient!,
            parsedChannel,
            request.Content,
            request.NotificationTemplateId,
            idempotencyScope!,
            request.IdempotencyKey!);

        try
        {
            var result = await CreateAsync(command, cancellationToken);
            var response = new CreateNotificationResponseDto(result.Notification.Id, result.Notification.Status.ToString());
            return result.Created
                ? NotificationAppResult<CreateNotificationResponseDto>.Created(response)
                : NotificationAppResult<CreateNotificationResponseDto>.Ok(response);
        }
        catch (ArgumentException ex)
        {
            return NotificationAppResult<CreateNotificationResponseDto>.Validation(
                new Dictionary<string, string[]>
                {
                    ["request"] = [ex.Message]
                });
        }
    }

    private async Task<CreateNotificationResult> CreateAsync(CreateNotificationCommand command, CancellationToken cancellationToken)
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
