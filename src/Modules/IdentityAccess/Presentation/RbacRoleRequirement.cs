using Microsoft.AspNetCore.Authorization;

namespace IdentityAccess.Presentation;

public sealed class RbacRoleRequirement : IAuthorizationRequirement
{
    public RbacRoleRequirement(params string[] allowedRoles)
    {
        ArgumentNullException.ThrowIfNull(allowedRoles);
        AllowedRoles = allowedRoles;
    }

    public IReadOnlyCollection<string> AllowedRoles { get; }
}
