using Microsoft.AspNetCore.Authorization;

namespace IdentityAccess.Presentation;

/// <summary>
/// Handler kiểm tra yêu cầu phân quyền theo vai trò RBAC.
/// </summary>
public sealed class RbacRoleAuthorizationHandler : AuthorizationHandler<RbacRoleRequirement>
{
    /// <summary>
    /// Xử lý kiểm tra vai trò của người dùng với yêu cầu phân quyền.
    /// </summary>
    /// <param name="context">Ngữ cảnh phân quyền.</param>
    /// <param name="requirement">Yêu cầu phân quyền theo vai trò.</param>
    /// <returns>Task hoàn thành sau khi đánh giá yêu cầu.</returns>
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        RbacRoleRequirement requirement)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(requirement);

        if (requirement.AllowedRoles.Any(role => context.User.IsInRole(role)))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
