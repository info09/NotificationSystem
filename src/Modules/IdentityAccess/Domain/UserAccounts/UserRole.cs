namespace IdentityAccess.Domain.UserAccounts;

/// <summary>
/// Vai trò của Tài khoản người dùng.
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Quản trị viên hệ thống.
    /// </summary>
    SystemAdministrator = 1,

    /// <summary>
    /// Nhân viên vận hành.
    /// </summary>
    BusinessOperator = 2,

    /// <summary>
    /// Tích hợp từ hệ thống bên ngoài.
    /// </summary>
    IntegrationClient = 3
}
