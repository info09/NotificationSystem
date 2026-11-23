namespace BuildingBlocks.SharedKernel.Constants;

/// <summary>
/// Hằng số tên policy RBAC dùng trong toàn hệ thống.
/// </summary>
public static class RbacPolicies
{
    /// <summary>
    /// Policy dành cho vai trò Quản trị viên hệ thống.
    /// </summary>
    public const string SystemAdministrator = "SystemAdministrator";

    /// <summary>
    /// Policy dành cho vai trò Nhân viên vận hành.
    /// </summary>
    public const string BusinessOperator = "BusinessOperator";

    /// <summary>
    /// Policy dành cho vai trò Nhà phát triển tích hợp.
    /// </summary>
    public const string IntegrationClient = "IntegrationClient";

    /// <summary>
    /// Policy cho phép đọc dữ liệu Thông báo theo quyền.
    /// </summary>
    public const string NotificationRead = "NotificationRead";
}
