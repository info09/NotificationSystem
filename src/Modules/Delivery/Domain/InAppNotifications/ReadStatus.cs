namespace Delivery.Domain.InAppNotifications;

/// <summary>
/// Trạng thái đọc của Thông báo trong ứng dụng.
/// </summary>
public enum ReadStatus
{
    /// <summary>
    /// Thông báo chưa được đọc.
    /// </summary>
    Unread = 0,

    /// <summary>
    /// Thông báo đã được đọc.
    /// </summary>
    Read = 1
}
