using BuildingBlocks.SharedKernel.Constants;
using IdentityAccess.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityAccess.Presentation;

/// <summary>
/// Phương thức mở rộng đăng ký cấu hình phân quyền RBAC cho module IdentityAccess.
/// </summary>
public static class RbacAuthorizationExtensions
{
    /// <summary>
    /// Đăng ký policy RBAC và các handler phân quyền.
    /// </summary>
    /// <param name="services">Tập hợp dịch vụ của ứng dụng.</param>
    /// <returns>Tập hợp dịch vụ để chuỗi cấu hình.</returns>
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

            options.AddPolicy(RbacPolicies.NotificationRead, policy =>
                policy.Requirements.Add(new RbacRoleRequirement(
                    RbacRoles.SystemAdministrator,
                    RbacRoles.BusinessOperator,
                    RbacRoles.IntegrationClient)));

            options.AddPolicy(RbacPolicies.DeadLetterQueueRead, policy =>
                policy.Requirements.Add(new RbacRoleRequirement(
                    RbacRoles.SystemAdministrator,
                    RbacRoles.BusinessOperator)));

            options.AddPolicy(RbacPolicies.DeadLetterQueueResend, policy =>
                policy.Requirements.Add(new RbacRoleRequirement(
                    RbacRoles.SystemAdministrator,
                    RbacRoles.BusinessOperator)));
        });

        services.AddSingleton<IAuthorizationHandler, RbacRoleAuthorizationHandler>();
        services.AddSingleton<IAuthorizationMiddlewareResultHandler, RbacProblemDetailsAuthorizationMiddlewareResultHandler>();

        return services;
    }
}
