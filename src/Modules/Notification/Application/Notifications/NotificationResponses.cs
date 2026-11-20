namespace Notification.Application.Notifications;

public sealed record CreateNotificationResponseDto(Guid NotificationId, string Status);

public sealed record NotificationItemResponseDto(
    Guid Id,
    string Recipient,
    string Channel,
    string Status,
    string? Content,
    Guid? NotificationTemplateId,
    DateTime CreatedAt);

public sealed record GetNotificationsResponseDto(
    IReadOnlyList<NotificationItemResponseDto> Items,
    int TotalCount,
    int Page,
    int PageSize);