using Microsoft.AspNetCore.Authorization;

namespace IdentityAccess.Presentation;

/// <summary>
/// Yêu cầu phân quyền theo danh sách vai trò được phép.
/// </summary>
public sealed class RbacRoleRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Khởi tạo yêu cầu với danh sách vai trò được phép.
    /// </summary>
    /// <param name="allowedRoles">Các vai trò được phép truy cập.</param>
    public RbacRoleRequirement(params string[] allowedRoles)
    {
        ArgumentNullException.ThrowIfNull(allowedRoles);
        AllowedRoles = allowedRoles;
    }

    /// <summary>
    /// Danh sách vai trò được phép.
    /// </summary>
    public IReadOnlyCollection<string> AllowedRoles { get; }
}
