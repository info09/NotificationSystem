namespace Notification.Application.Notifications;

public sealed record NotificationCallerContext(string? Subject, IReadOnlyCollection<string> Roles)
{
    public static NotificationCallerContext Empty { get; } = new(null, Array.Empty<string>());
}