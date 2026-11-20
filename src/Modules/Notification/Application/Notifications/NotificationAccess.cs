namespace Notification.Application.Notifications;

internal static class NotificationAccess
{
    private static readonly string[] PrivilegedRoles = ["SystemAdministrator", "BusinessOperator"];

    public static string? GetScopeRestriction(NotificationCallerContext? caller)
    {
        if (caller is not null && caller.Roles.Any(role => PrivilegedRoles.Contains(role, StringComparer.Ordinal)))
        {
            return null;
        }

        return caller?.Subject;
    }
}