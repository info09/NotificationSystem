namespace IdentityAccess.Domain.UserAccounts;

/// <summary>
/// Trạng thái Tài khoản người dùng.
/// </summary>
public enum UserAccountStatus
{
    /// <summary>
    /// Tài khoản đang hoạt động.
    /// </summary>
    Active = 1,

    /// <summary>
    /// Tài khoản bị khóa.
    /// </summary>
    Locked = 2,

    /// <summary>
    /// Tài khoản đã bị xóa.
    /// </summary>
    Deleted = 3
}
