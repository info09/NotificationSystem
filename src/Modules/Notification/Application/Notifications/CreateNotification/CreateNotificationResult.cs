namespace Notification.Application.Notifications.CreateNotification;

/// <summary>
/// Kết quả tạo Thông báo ở tầng xử lý nghiệp vụ.
/// </summary>
/// <param name="Notification">Thông báo sau khi xử lý.</param>
/// <param name="Created">Cho biết Thông báo có được tạo mới hay không.</param>
public sealed record CreateNotificationResult(NotificationDto Notification, bool Created);