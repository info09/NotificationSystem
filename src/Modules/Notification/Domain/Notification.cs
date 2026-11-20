using BuildingBlocks.SharedKernel.Entity;

namespace Notification.Domain;

public sealed class Notification : Entity
{
    private Notification()
    {
        Recipient = null!;
        IdempotencyScope = string.Empty;
        IdempotencyKey = string.Empty;
    }

    private Notification(
        Guid id,
        Recipient recipient,
        NotificationChannel channel,
        NotificationStatus status,
        string? content,
        Guid? notificationTemplateId,
        string idempotencyScope,
        string idempotencyKey,
        DateTime createdAt)
    {
        Id = id;
        Recipient = recipient;
        Channel = channel;
        Status = status;
        Content = content;
        NotificationTemplateId = notificationTemplateId;
        IdempotencyScope = idempotencyScope;
        IdempotencyKey = idempotencyKey;
        CreatedAt = createdAt;
    }

    public Recipient Recipient { get; private set; }

    public NotificationChannel Channel { get; private set; }

    public NotificationStatus Status { get; private set; }

    public string? Content { get; private set; }

    public Guid? NotificationTemplateId { get; private set; }

    public string IdempotencyScope { get; private set; }

    public string IdempotencyKey { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public static Notification Create(
        Guid id,
        Recipient recipient,
        NotificationChannel channel,
        string? content,
        Guid? notificationTemplateId,
        string idempotencyScope,
        string idempotencyKey,
        DateTime? createdAt = null)
    {
        ArgumentNullException.ThrowIfNull(recipient);

        if (string.IsNullOrWhiteSpace(content) && notificationTemplateId is null)
        {
            throw new ArgumentException("Notification content or notification template reference is required.");
        }

        if (string.IsNullOrWhiteSpace(idempotencyScope))
        {
            throw new ArgumentException("Idempotency scope is required.", nameof(idempotencyScope));
        }

        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            throw new ArgumentException("Idempotency key is required.", nameof(idempotencyKey));
        }

        return new Notification(
            id,
            recipient,
            channel,
            NotificationStatus.Pending,
            content,
            notificationTemplateId,
            idempotencyScope,
            idempotencyKey,
            createdAt ?? DateTime.UtcNow);
    }

    public void MarkProcessing()
    {
        if (Status != NotificationStatus.Pending)
        {
            throw new InvalidOperationException("Only pending notifications can be marked as processing.");
        }

        Status = NotificationStatus.Processing;
    }

    public void MarkSent()
    {
        if (Status != NotificationStatus.Processing)
        {
            throw new InvalidOperationException("Only processing notifications can be marked as sent.");
        }

        Status = NotificationStatus.Sent;
    }

    public void MarkFailed()
    {
        if (Status != NotificationStatus.Processing)
        {
            throw new InvalidOperationException("Only processing notifications can be marked as failed.");
        }

        Status = NotificationStatus.Failed;
    }
}
