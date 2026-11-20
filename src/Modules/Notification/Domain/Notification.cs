namespace Notification.Domain;

public sealed class Notification
{
    private Notification(
        Guid id,
        Recipient recipient,
        NotificationChannel channel,
        string? content,
        Guid? notificationTemplateId)
    {
        Id = id;
        Recipient = recipient;
        Channel = channel;
        Content = content;
        NotificationTemplateId = notificationTemplateId;
        Status = NotificationStatus.Pending;
    }

    public Guid Id { get; }

    public Recipient Recipient { get; }

    public NotificationChannel Channel { get; }

    public NotificationStatus Status { get; private set; }

    public string? Content { get; }

    public Guid? NotificationTemplateId { get; }

    public static Notification Create(
        Guid id,
        Recipient recipient,
        NotificationChannel channel,
        string? content,
        Guid? notificationTemplateId)
    {
        ArgumentNullException.ThrowIfNull(recipient);

        if (string.IsNullOrWhiteSpace(content) && notificationTemplateId is null)
        {
            throw new ArgumentException("Notification content or notification template reference is required.");
        }

        return new Notification(id, recipient, channel, content, notificationTemplateId);
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
