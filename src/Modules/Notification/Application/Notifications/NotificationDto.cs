using NotificationChannel = Notification.Domain.NotificationChannel;
using NotificationStatus = Notification.Domain.NotificationStatus;

namespace Notification.Application.Notifications;

public sealed record NotificationDto(
    Guid Id,
    string Recipient,
    NotificationChannel Channel,
    NotificationStatus Status,
    string? Content,
    Guid? NotificationTemplateId);
