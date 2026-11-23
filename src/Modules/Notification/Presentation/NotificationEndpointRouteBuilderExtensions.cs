using BuildingBlocks.SharedKernel.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Notification.Application.Notifications;
using Notification.Application.Notifications.CreateNotification;
using Notification.Application.Notifications.GetNotificationById;
using Notification.Application.Notifications.GetNotifications;
using System.Security.Claims;

namespace Notification.Presentation;

/// <summary>
/// Phương thức mở rộng ánh xạ endpoint cho module Notification.
/// </summary>
public static class NotificationEndpointRouteBuilderExtensions
{
    private const int DefaultPage = 1;
    private const int DefaultPageSize = 20;

    /// <summary>
    /// Đăng ký các endpoint Notification vào route builder.
    /// </summary>
    /// <param name="endpoints">Route builder của ứng dụng.</param>
    /// <returns>Route builder để chuỗi cấu hình.</returns>
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

    /// <summary>
    /// Xử lý yêu cầu tạo Thông báo.
    /// </summary>
    /// <param name="request">Dữ liệu yêu cầu tạo Thông báo.</param>
    /// <param name="user">Thông tin người dùng hiện tại.</param>
    /// <param name="createNotificationUseCase">Use case tạo Thông báo.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả HTTP của thao tác tạo.</returns>
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

    /// <summary>
    /// Xử lý yêu cầu lấy danh sách Thông báo.
    /// </summary>
    /// <param name="user">Thông tin người dùng hiện tại.</param>
    /// <param name="getNotificationsUseCase">Use case lấy danh sách Thông báo.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <param name="status">Trạng thái cần lọc.</param>
    /// <param name="channel">Kênh gửi cần lọc.</param>
    /// <param name="createdFrom">Thời điểm tạo từ.</param>
    /// <param name="createdTo">Thời điểm tạo đến.</param>
    /// <param name="page">Số trang.</param>
    /// <param name="pageSize">Kích thước trang.</param>
    /// <returns>Kết quả HTTP của truy vấn danh sách.</returns>
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

    /// <summary>
    /// Xử lý yêu cầu lấy chi tiết Thông báo theo mã định danh.
    /// </summary>
    /// <param name="id">Mã định danh Thông báo.</param>
    /// <param name="user">Thông tin người dùng hiện tại.</param>
    /// <param name="getNotificationByIdUseCase">Use case lấy chi tiết Thông báo.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả HTTP của truy vấn chi tiết.</returns>
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

    /// <summary>
    /// Tạo ngữ cảnh người gọi từ thông tin claims.
    /// </summary>
    /// <param name="user">Thông tin người dùng hiện tại.</param>
    /// <returns>Ngữ cảnh người gọi.</returns>
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

    /// <summary>
    /// Chuyển kết quả tầng Application sang kết quả HTTP.
    /// </summary>
    /// <typeparam name="T">Kiểu dữ liệu trả về.</typeparam>
    /// <param name="result">Kết quả xử lý từ tầng Application.</param>
    /// <param name="createdLocationFactory">Hàm tạo đường dẫn tài nguyên khi tạo mới.</param>
    /// <returns>Kết quả HTTP tương ứng.</returns>
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