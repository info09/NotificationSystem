namespace Notification.Presentation;

public sealed record GetNotificationsResponse(
    IReadOnlyList<NotificationResponseItem> Items,
    int TotalCount,
    int Page,
    int PageSize);