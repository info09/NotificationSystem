namespace Notification.Application.Notifications.GetNotificationById;

public sealed record GetNotificationByIdAppRequest(Guid Id, NotificationCallerContext Caller);