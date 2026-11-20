using BuildingBlocks.SharedKernel.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Notification.Application.Notifications.CreateNotification;
using System.Security.Claims;
using NotificationChannel = global::Notification.Domain.NotificationChannel;

namespace Notification.Presentation;

public static class NotificationEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapNotificationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        endpoints.MapPost("/api/notifications", HandleCreateNotificationAsync)
            .RequireAuthorization(RbacPolicies.IntegrationClient);

        return endpoints;
    }

    private static async Task<IResult> HandleCreateNotificationAsync(
        CreateNotificationRequest? request,
        ClaimsPrincipal user,
        CreateNotificationUseCase createNotificationUseCase,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["request"] = ["Request body is required."]
            });
        }

        var validationErrors = ValidateRequest(request);
        if (validationErrors.Count > 0)
        {
            return Results.ValidationProblem(validationErrors);
        }

        var idempotencyScope = user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user.FindFirstValue("sub");

        if (string.IsNullOrWhiteSpace(idempotencyScope))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["idempotencyScope"] = ["Authenticated caller scope could not be determined."]
            });
        }

        if (!TryParseChannel(request.Channel!, out var channel))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["channel"] = ["Channel must be 'email' or 'inapp'."]
            });
        }

        var command = new CreateNotificationCommand(
            request.Recipient!,
            channel,
            request.Content,
            request.NotificationTemplateId,
            idempotencyScope,
            request.IdempotencyKey!);

        try
        {
            var result = await createNotificationUseCase.HandleAsync(command, cancellationToken);
            var response = new CreateNotificationResponse(result.Notification.Id, result.Notification.Status.ToString());

            return result.Created
                ? Results.Created($"/api/notifications/{response.NotificationId}", response)
                : Results.Ok(response);
        }
        catch (ArgumentException ex)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["request"] = [ex.Message]
            });
        }
    }

    private static Dictionary<string, string[]> ValidateRequest(CreateNotificationRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.IdempotencyKey))
        {
            errors["idempotencyKey"] = ["IdempotencyKey is required."];
        }

        if (string.IsNullOrWhiteSpace(request.Recipient))
        {
            errors["recipient"] = ["Recipient is required."];
        }

        if (string.IsNullOrWhiteSpace(request.Channel))
        {
            errors["channel"] = ["Channel is required."];
        }
        else if (!TryParseChannel(request.Channel, out _))
        {
            errors["channel"] = ["Channel must be 'email' or 'inapp'."];
        }

        if (string.IsNullOrWhiteSpace(request.Content) && request.NotificationTemplateId is null)
        {
            errors["content"] = ["Content or notificationTemplateId is required."];
        }

        return errors;
    }

    private static bool TryParseChannel(string channel, out NotificationChannel parsedChannel)
    {
        if (string.Equals(channel, "email", StringComparison.OrdinalIgnoreCase))
        {
            parsedChannel = NotificationChannel.Email;
            return true;
        }

        if (string.Equals(channel, "inapp", StringComparison.OrdinalIgnoreCase))
        {
            parsedChannel = NotificationChannel.InApp;
            return true;
        }

        parsedChannel = default;
        return false;
    }
}