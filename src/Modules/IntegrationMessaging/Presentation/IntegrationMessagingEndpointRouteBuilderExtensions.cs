using System.Text.Json;
using BuildingBlocks.SharedKernel.Api;
using BuildingBlocks.SharedKernel.Constants;
using IntegrationMessaging.Application;
using IntegrationMessaging.Application.IntegrationEvents;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace IntegrationMessaging.Presentation;

/// <summary>
/// Phương thức mở rộng ánh xạ endpoint cho module IntegrationMessaging.
/// </summary>
public static class IntegrationMessagingEndpointRouteBuilderExtensions
{
    /// <summary>
    /// Đăng ký các endpoint IntegrationMessaging vào route builder.
    /// </summary>
    /// <param name="endpoints">Route builder của ứng dụng.</param>
    /// <returns>Route builder để chuỗi cấu hình.</returns>
    public static IEndpointRouteBuilder MapIntegrationMessagingEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        endpoints.MapPost("/api/integration-events", HandleReceiveIntegrationEventAsync)
            .WithTags("IntegrationMessaging")
            .RequireAuthorization(RbacPolicies.IntegrationClient);

        return endpoints;
    }

    /// <summary>
    /// Xử lý yêu cầu tiếp nhận Sự kiện tích hợp.
    /// </summary>
    /// <param name="request">Yêu cầu tiếp nhận Sự kiện tích hợp.</param>
    /// <param name="useCase">Use case tiếp nhận Sự kiện tích hợp.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả HTTP.</returns>
    private static async Task<IResult> HandleReceiveIntegrationEventAsync(
        ReceiveIntegrationEventRequest? request,
        ReceiveIntegrationEventUseCase useCase,
        CancellationToken cancellationToken)
    {
        var appRequest = request is null
            ? null
            : new ReceiveIntegrationEventAppRequest(
                request.MessageId,
                request.EventId,
                request.EventType,
                request.OccurredAt,
                request.Source,
                request.IdempotencyKey,
                request.Version,
                request.Payload);

        var result = await useCase.HandleAsync(appRequest, cancellationToken);
        return ToHttpResult(result, value => $"/api/integration-events/{value.Id}");
    }

    /// <summary>
    /// Chuyển kết quả tầng Application sang kết quả HTTP.
    /// </summary>
    /// <typeparam name="T">Kiểu dữ liệu trả về.</typeparam>
    /// <param name="result">Kết quả xử lý từ tầng Application.</param>
    /// <param name="createdLocationFactory">Hàm tạo đường dẫn tài nguyên mới.</param>
    /// <returns>Kết quả HTTP tương ứng.</returns>
    private static IResult ToHttpResult<T>(IntegrationMessagingAppResult<T> result, Func<T, string>? createdLocationFactory = null)
    {
        return result.Status switch
        {
            IntegrationMessagingAppStatus.Ok => Results.Ok(result.Value),
            IntegrationMessagingAppStatus.Created => createdLocationFactory is not null && result.Value is not null
                ? Results.Created(createdLocationFactory(result.Value), result.Value)
                : Results.Ok(result.Value),
            IntegrationMessagingAppStatus.ValidationError => Results.ValidationProblem(
                result.ValidationErrors ?? new Dictionary<string, string[]>(),
                title: "Validation Error",
                detail: "One or more validation errors occurred.",
                statusCode: StatusCodes.Status400BadRequest,
                extensions: new Dictionary<string, object?> { ["code"] = ApiProblemCodes.ValidationError }),
            IntegrationMessagingAppStatus.NotFound => Results.Problem(
                title: "Not Found",
                detail: "The requested resource was not found.",
                statusCode: StatusCodes.Status404NotFound,
                extensions: new Dictionary<string, object?> { ["code"] = ApiProblemCodes.NotFound }),
            IntegrationMessagingAppStatus.Conflict => Results.Problem(
                title: "Conflict",
                detail: "The requested operation conflicts with the current state.",
                statusCode: StatusCodes.Status409Conflict,
                extensions: new Dictionary<string, object?> { ["code"] = ApiProblemCodes.Conflict }),
            _ => Results.Problem(
                title: "Unexpected Error",
                detail: "Unexpected application result status.",
                statusCode: StatusCodes.Status500InternalServerError,
                extensions: new Dictionary<string, object?> { ["code"] = ApiProblemCodes.UnexpectedError })
        };
    }
}

/// <summary>
/// Yêu cầu tiếp nhận Sự kiện tích hợp.
/// </summary>
/// <param name="MessageId">Mã định danh Thông điệp.</param>
/// <param name="EventId">Mã định danh Sự kiện tích hợp.</param>
/// <param name="EventType">Loại Sự kiện tích hợp.</param>
/// <param name="OccurredAt">Thời điểm phát sinh sự kiện.</param>
/// <param name="Source">Hệ thống nguồn.</param>
/// <param name="IdempotencyKey">Khóa chống trùng.</param>
/// <param name="Version">Phiên bản envelope.</param>
/// <param name="Payload">Nội dung payload.</param>
public sealed record ReceiveIntegrationEventRequest(
    Guid? MessageId,
    Guid? EventId,
    string? EventType,
    DateTime? OccurredAt,
    string? Source,
    string? IdempotencyKey,
    string? Version,
    JsonElement? Payload);
