using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;

namespace IdentityAccess.Presentation;

public sealed class RbacProblemDetailsAuthorizationMiddlewareResultHandler
    : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();

    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        ArgumentNullException.ThrowIfNull(next);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(policy);
        ArgumentNullException.ThrowIfNull(authorizeResult);

        if (authorizeResult.Challenged)
        {
            await WriteProblemDetailsAsync(
                context,
                StatusCodes.Status401Unauthorized,
                "Unauthorized",
                "Authentication is required to access this resource.");
            return;
        }

        if (authorizeResult.Forbidden)
        {
            await WriteProblemDetailsAsync(
                context,
                StatusCodes.Status403Forbidden,
                "Forbidden",
                "You do not have permission to access this resource.");
            return;
        }

        await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
    }

    private static async Task WriteProblemDetailsAsync(
        HttpContext context,
        int statusCode,
        string title,
        string detail)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.StatusCode = statusCode;
        await Results.Problem(
                title: title,
                detail: detail,
                statusCode: statusCode,
                instance: context.Request.Path)
            .ExecuteAsync(context);
    }
}
