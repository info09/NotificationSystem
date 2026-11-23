namespace Notification.Application.Notifications;

/// <summary>
/// Hỗ trợ xác định phạm vi truy cập dữ liệu Thông báo theo ngữ cảnh người gọi.
/// </summary>
internal static class NotificationAccess
{
    private static readonly string[] PrivilegedRoles = ["SystemAdministrator", "BusinessOperator"];

    /// <summary>
    /// Lấy giới hạn phạm vi truy vấn dựa trên vai trò và định danh người gọi.
    /// </summary>
    /// <param name="caller">Ngữ cảnh người gọi.</param>
    /// <returns>Null nếu có quyền toàn cục; ngược lại là định danh phạm vi.</returns>
    public static string? GetScopeRestriction(NotificationCallerContext? caller)
    {
        if (caller is not null && caller.Roles.Any(role => PrivilegedRoles.Contains(role, StringComparer.Ordinal)))
        {
            return null;
        }

        return caller?.Subject;
    }
}