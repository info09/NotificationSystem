using NotificationChannel = global::Notification.Domain.NotificationChannel;
using NotificationStatus = global::Notification.Domain.NotificationStatus;

namespace Notification.Application.Notifications.GetNotifications;

public sealed record GetNotificationsQuery(
    NotificationStatus? Status,
    NotificationChannel? Channel,
    DateTime? CreatedFromUtc,
    DateTime? CreatedToUtc,
    int Page,
    int PageSize,
    string? ScopeRestriction);