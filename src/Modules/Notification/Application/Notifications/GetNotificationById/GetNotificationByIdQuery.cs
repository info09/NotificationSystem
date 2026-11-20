namespace Notification.Application.Notifications.GetNotificationById;

public sealed record GetNotificationByIdQuery(Guid Id, string? ScopeRestriction);