using DomainNotification = Notification.Domain.Notification;

namespace Notification.Application.Notifications;

/// <summary>
/// Port ghi bản ghi Outbox cho nghiệp vụ Thông báo.
/// </summary>
public interface INotificationOutboxWriter
{
    /// <summary>
    /// Thêm bản ghi Outbox cho sự kiện tạo Thông báo.
    /// </summary>
    /// <param name="notification">Thông báo vừa được tạo.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    void AddNotificationCreated(DomainNotification notification);
}
