namespace Notification.Domain;

/// <summary>
/// Kênh gửi Thông báo.
/// </summary>
public enum NotificationChannel
{
    /// <summary>
    /// Kênh email.
    /// </summary>
    Email = 0,

    /// <summary>
    /// Kênh trong ứng dụng.
    /// </summary>
    InApp = 1
}
