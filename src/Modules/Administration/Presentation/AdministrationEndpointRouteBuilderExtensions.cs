using Administration.Application;
using Administration.Application.DeadLetterQueue;
using BuildingBlocks.SharedKernel.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

namespace Administration.Presentation;

/// <summary>
/// Phương thức mở rộng ánh xạ endpoint cho module Administration.
/// </summary>
public static class AdministrationEndpointRouteBuilderExtensions
{
    /// <summary>
    /// Đăng ký các endpoint Administration vào route builder.
    /// </summary>
    /// <param name="endpoints">Route builder của ứng dụng.</param>
    /// <returns>Route builder để chuỗi cấu hình.</returns>
    public static IEndpointRouteBuilder MapAdministrationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        endpoints.MapGet("/api/admin/dead-letter-queues/{queueName}/messages", HandleGetDeadLetterMessagesAsync)
            .WithTags("Administration")
            .RequireAuthorization(RbacPolicies.DeadLetterQueueRead);

        endpoints.MapPost("/api/admin/dead-letter-queues/{queueName}/messages/{messageId}/resend", HandleResendDeadLetterMessageAsync)
            .WithTags("Administration")
            .RequireAuthorization(RbacPolicies.DeadLetterQueueResend);

        return endpoints;
    }

    /// <summary>
    /// Xử lý yêu cầu truy vấn danh sách Thông điệp trong Hàng đợi thư chết.
    /// </summary>
    /// <param name="queueName">Tên Hàng đợi thư chết.</param>
    /// <param name="limit">Số lượng tối đa.</param>
    /// <param name="useCase">Use case truy vấn Hàng đợi thư chết.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả HTTP của truy vấn.</returns>
    private static async Task<IResult> HandleGetDeadLetterMessagesAsync(
        string queueName,
        int? limit,
        GetDeadLetterMessagesUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.HandleAsync(queueName, limit, cancellationToken);
        return ToHttpResult(result);
    }

    /// <summary>
    /// Xử lý yêu cầu gửi lại Thông điệp từ Hàng đợi thư chết.
    /// </summary>
    /// <param name="queueName">Tên Hàng đợi thư chết.</param>
    /// <param name="messageId">Mã định danh Thông điệp.</param>
    /// <param name="request">Dữ liệu yêu cầu gửi lại.</param>
    /// <param name="user">Thông tin người dùng hiện tại.</param>
    /// <param name="useCase">Use case gửi lại Thông điệp.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả HTTP của thao tác gửi lại.</returns>
    private static async Task<IResult> HandleResendDeadLetterMessageAsync(
        string queueName,
        string messageId,
        ResendDeadLetterMessageRequest? request,
        ClaimsPrincipal user,
        ResendDeadLetterMessageUseCase useCase,
        CancellationToken cancellationToken)
    {
        var caller = CreateCallerContext(user);
        var result = await useCase.HandleAsync(queueName, messageId, request, caller, cancellationToken);
        return ToHttpResult(result);
    }

    /// <summary>
    /// Tạo ngữ cảnh người gọi từ thông tin claims.
    /// </summary>
    /// <param name="user">Thông tin người dùng hiện tại.</param>
    /// <returns>Ngữ cảnh người gọi.</returns>
    private static AdministrationCallerContext CreateCallerContext(ClaimsPrincipal user)
    {
        var subject = user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user.FindFirstValue("sub");
        var roles = user.Claims
            .Where(claim => claim.Type == ClaimTypes.Role)
            .Select(claim => claim.Value)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var correlationId = user.FindFirstValue("correlation_id");

        return new AdministrationCallerContext(subject, roles, correlationId);
    }

    /// <summary>
    /// Chuyển kết quả tầng Application sang kết quả HTTP.
    /// </summary>
    /// <typeparam name="T">Kiểu dữ liệu trả về.</typeparam>
    /// <param name="result">Kết quả xử lý từ tầng Application.</param>
    /// <returns>Kết quả HTTP tương ứng.</returns>
    private static IResult ToHttpResult<T>(AdministrationAppResult<T> result)
    {
        return result.Status switch
        {
            AdministrationAppStatus.Ok => Results.Ok(result.Value),
            AdministrationAppStatus.Created => Results.Ok(result.Value),
            AdministrationAppStatus.ValidationError => Results.ValidationProblem(result.ValidationErrors ?? new Dictionary<string, string[]>()),
            AdministrationAppStatus.NotFound => Results.NotFound(),
            AdministrationAppStatus.Conflict => Results.Conflict(),
            _ => Results.Problem("Unexpected application result status.")
        };
    }
}
