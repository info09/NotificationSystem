namespace Notification.Application.Notifications.GetNotifications;

public sealed record GetNotificationsAppRequest(
    string? Status,
    string? Channel,
    string? CreatedFrom,
    string? CreatedTo,
    int Page,
    int PageSize,
    NotificationCallerContext Caller);