namespace IdentityAccess.Domain;

/// <summary>
/// Định nghĩa các vai trò RBAC dùng trong module IdentityAccess.
/// </summary>
public static class RbacRoles
{
    /// <summary>
    /// Vai trò Quản trị viên hệ thống.
    /// </summary>
    public const string SystemAdministrator = "SystemAdministrator";

    /// <summary>
    /// Vai trò Nhân viên vận hành.
    /// </summary>
    public const string BusinessOperator = "BusinessOperator";

    /// <summary>
    /// Vai trò Nhà phát triển tích hợp.
    /// </summary>
    public const string IntegrationClient = "IntegrationClient";

    /// <summary>
    /// Danh sách tất cả vai trò hợp lệ.
    /// </summary>
    public static readonly IReadOnlyCollection<string> All =
    [
        SystemAdministrator,
        BusinessOperator,
        IntegrationClient
    ];
}
