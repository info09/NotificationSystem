using System.Globalization;
using NotificationChannel = global::Notification.Domain.NotificationChannel;

namespace Notification.Application.Notifications;

/// <summary>
/// Hỗ trợ phân tích dữ liệu đầu vào cho Notification.
/// </summary>
internal static class NotificationParsing
{
    /// <summary>
    /// Phân tích chuỗi kênh gửi thành giá trị enum kênh Thông báo.
    /// </summary>
    /// <param name="channel">Chuỗi kênh gửi.</param>
    /// <param name="parsedChannel">Kênh Thông báo đã phân tích.</param>
    /// <returns>True nếu phân tích thành công; ngược lại false.</returns>
    public static bool TryParseChannel(string channel, out NotificationChannel parsedChannel)
    {
        if (string.Equals(channel, "email", StringComparison.OrdinalIgnoreCase))
        {
            parsedChannel = NotificationChannel.Email;
            return true;
        }

        if (string.Equals(channel, "inapp", StringComparison.OrdinalIgnoreCase))
        {
            parsedChannel = NotificationChannel.InApp;
            return true;
        }

        parsedChannel = default;
        return false;
    }

    /// <summary>
    /// Phân tích chuỗi thời gian thành giá trị UTC.
    /// </summary>
    /// <param name="value">Chuỗi thời gian đầu vào.</param>
    /// <param name="utcDateTime">Giá trị thời gian UTC đã phân tích.</param>
    /// <returns>True nếu phân tích thành công; ngược lại false.</returns>
    public static bool TryParseUtcDateTime(string value, out DateTime utcDateTime)
    {
        if (DateTimeOffset.TryParse(
            value,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
            out var dateTimeOffset))
        {
            utcDateTime = dateTimeOffset.UtcDateTime;
            return true;
        }

        utcDateTime = default;
        return false;
    }
}