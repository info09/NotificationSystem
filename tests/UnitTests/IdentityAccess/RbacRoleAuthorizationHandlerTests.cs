using IdentityAccess.Domain;
using IdentityAccess.Presentation;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace UnitTests.IdentityAccess;

public sealed class RbacRoleAuthorizationHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenUserHasAllowedRole_Succeeds()
    {
        var requirement = new RbacRoleRequirement(RbacRoles.SystemAdministrator);
        var user = CreatePrincipalWithRole(RbacRoles.SystemAdministrator);
        var context = new AuthorizationHandlerContext([requirement], user, resource: null);
        var handler = new RbacRoleAuthorizationHandler();

        await handler.HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task HandleAsync_WhenUserHasDifferentRole_DoesNotSucceed()
    {
        var requirement = new RbacRoleRequirement(RbacRoles.SystemAdministrator);
        var user = CreatePrincipalWithRole(RbacRoles.BusinessOperator);
        var context = new AuthorizationHandlerContext([requirement], user, resource: null);
        var handler = new RbacRoleAuthorizationHandler();

        await handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async Task HandleAsync_WhenUserHasNoRoles_DoesNotSucceed()
    {
        var requirement = new RbacRoleRequirement(RbacRoles.IntegrationClient);
        var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "user")], "Test"));
        var context = new AuthorizationHandlerContext([requirement], user, resource: null);
        var handler = new RbacRoleAuthorizationHandler();

        await handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    private static ClaimsPrincipal CreatePrincipalWithRole(string role)
    {
        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.Name, "test-user"), new Claim(ClaimTypes.Role, role)],
            authenticationType: "Test");

        return new ClaimsPrincipal(identity);
    }
}
