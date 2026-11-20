using BuildingBlocks.SharedKernel.Constants;
using IdentityAccess.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityAccess.Presentation;

public static class RbacAuthorizationExtensions
{
    public static IServiceCollection AddIdentityAccessRbacAuthorization(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddAuthorization(options =>
        {
            options.AddPolicy(RbacPolicies.SystemAdministrator, policy =>
                policy.Requirements.Add(new RbacRoleRequirement(RbacRoles.SystemAdministrator)));

            options.AddPolicy(RbacPolicies.BusinessOperator, policy =>
                policy.Requirements.Add(new RbacRoleRequirement(RbacRoles.BusinessOperator)));

            options.AddPolicy(RbacPolicies.IntegrationClient, policy =>
                policy.Requirements.Add(new RbacRoleRequirement(RbacRoles.IntegrationClient)));
        });

        services.AddSingleton<IAuthorizationHandler, RbacRoleAuthorizationHandler>();
        services.AddSingleton<IAuthorizationMiddlewareResultHandler, RbacProblemDetailsAuthorizationMiddlewareResultHandler>();

        return services;
    }
}
