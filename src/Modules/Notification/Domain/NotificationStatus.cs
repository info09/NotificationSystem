namespace Notification.Domain;

/// <summary>
/// Trạng thái vòng đời của Thông báo.
/// </summary>
public enum NotificationStatus
{
    /// <summary>
    /// Thông báo đang chờ xử lý.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Thông báo đang được xử lý.
    /// </summary>
    Processing = 1,

    /// <summary>
    /// Thông báo đã gửi thành công.
    /// </summary>
    Sent = 2,

    /// <summary>
    /// Thông báo gửi thất bại.
    /// </summary>
    Failed = 3
}
