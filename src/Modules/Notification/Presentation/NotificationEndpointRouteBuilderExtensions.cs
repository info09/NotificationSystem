using System.Security.Claims;
using BuildingBlocks.SharedKernel.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Notification.Application.Notifications;
using Notification.Application.Notifications.CreateNotification;
using Notification.Application.Notifications.GetNotificationById;
using Notification.Application.Notifications.GetNotifications;

namespace Notification.Presentation;

public static class NotificationEndpointRouteBuilderExtensions
{
    private const int DefaultPage = 1;
    private const int DefaultPageSize = 20;

    public static IEndpointRouteBuilder MapNotificationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        endpoints.MapPost("/api/notifications", HandleCreateNotificationAsync)
            .WithTags("Notification")
            .RequireAuthorization(RbacPolicies.IntegrationClient);

        endpoints.MapGet("/api/notifications", HandleGetNotificationsAsync)
            .WithTags("Notification")
            .RequireAuthorization(RbacPolicies.NotificationRead);

        endpoints.MapGet("/api/notifications/{id:guid}", HandleGetNotificationByIdAsync)
            .WithTags("Notification")
            .RequireAuthorization(RbacPolicies.NotificationRead);

        return endpoints;
    }

    private static async Task<IResult> HandleCreateNotificationAsync(
        CreateNotificationRequest? request,
        ClaimsPrincipal user,
        CreateNotificationUseCase createNotificationUseCase,
        CancellationToken cancellationToken)
    {
        var appRequest = request is null
            ? null
            : new CreateNotificationAppRequest(
                request.IdempotencyKey,
                request.Recipient,
                request.Channel,
                request.Content,
                request.NotificationTemplateId,
                CreateCallerContext(user));

        var result = await createNotificationUseCase.HandleAsync(appRequest, cancellationToken);
        return ToHttpResult(result, createdLocationFactory: value => $"/api/notifications/{value.NotificationId}");
    }

    private static async Task<IResult> HandleGetNotificationsAsync(
        ClaimsPrincipal user,
        GetNotificationsUseCase getNotificationsUseCase,
        CancellationToken cancellationToken,
        string? status = null,
        string? channel = null,
        string? createdFrom = null,
        string? createdTo = null,
        int page = DefaultPage,
        int pageSize = DefaultPageSize)
    {
        var request = new GetNotificationsAppRequest(
            status,
            channel,
            createdFrom,
            createdTo,
            page,
            pageSize,
            CreateCallerContext(user));

        var result = await getNotificationsUseCase.HandleAsync(request, cancellationToken);
        return ToHttpResult(result);
    }

    private static async Task<IResult> HandleGetNotificationByIdAsync(
        Guid id,
        ClaimsPrincipal user,
        GetNotificationByIdUseCase getNotificationByIdUseCase,
        CancellationToken cancellationToken)
    {
        var request = new GetNotificationByIdAppRequest(id, CreateCallerContext(user));
        var result = await getNotificationByIdUseCase.HandleAsync(request, cancellationToken);
        return ToHttpResult(result);
    }

    private static NotificationCallerContext CreateCallerContext(ClaimsPrincipal user)
    {
        var subject = user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user.FindFirstValue("sub");
        var roles = user.Claims
            .Where(claim => claim.Type == ClaimTypes.Role)
            .Select(claim => claim.Value)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        return new NotificationCallerContext(subject, roles);
    }

    private static IResult ToHttpResult<T>(NotificationAppResult<T> result, Func<T, string>? createdLocationFactory = null)
    {
        return result.Status switch
        {
            NotificationAppStatus.Ok => Results.Ok(result.Value),
            NotificationAppStatus.Created => createdLocationFactory is not null && result.Value is not null
                ? Results.Created(createdLocationFactory(result.Value), result.Value)
                : Results.Ok(result.Value),
            NotificationAppStatus.ValidationError => Results.ValidationProblem(result.ValidationErrors ?? new Dictionary<string, string[]>()),
            NotificationAppStatus.NotFound => Results.NotFound(),
            _ => Results.Problem("Unexpected application result status.")
        };
    }
}