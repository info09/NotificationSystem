namespace Notification.Application.Notifications;

public enum NotificationAppStatus
{
    Ok,
    Created,
    ValidationError,
    NotFound
}

public sealed record NotificationAppResult<T>(
    NotificationAppStatus Status,
    T? Value,
    IReadOnlyDictionary<string, string[]>? ValidationErrors)
{
    public static NotificationAppResult<T> Ok(T value) =>
        new(NotificationAppStatus.Ok, value, null);

    public static NotificationAppResult<T> Created(T value) =>
        new(NotificationAppStatus.Created, value, null);

    public static NotificationAppResult<T> Validation(IReadOnlyDictionary<string, string[]> errors) =>
        new(NotificationAppStatus.ValidationError, default, errors);

    public static NotificationAppResult<T> NotFound() =>
        new(NotificationAppStatus.NotFound, default, null);
}