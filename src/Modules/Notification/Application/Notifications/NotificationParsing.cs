using System.Globalization;
using NotificationChannel = global::Notification.Domain.NotificationChannel;

namespace Notification.Application.Notifications;

internal static class NotificationParsing
{
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