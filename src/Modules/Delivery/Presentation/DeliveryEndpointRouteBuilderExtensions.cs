using BuildingBlocks.SharedKernel.Constants;
using Delivery.Application.DeliveryLogs;
using Delivery.Application.DeliveryLogs.GetNotificationDeliveryHistory;
using Delivery.Application.InAppNotifications;
using Delivery.Application.InAppNotifications.GetInAppNotificationById;
using Delivery.Application.InAppNotifications.GetInAppNotifications;
using Delivery.Application.InAppNotifications.GetUnreadInAppNotificationCount;
using Delivery.Application.InAppNotifications.MarkInAppNotificationAsRead;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

namespace Delivery.Presentation;

/// <summary>
/// Phương thức mở rộng ánh xạ endpoint cho module Delivery.
/// </summary>
public static class DeliveryEndpointRouteBuilderExtensions
{
    private const int DefaultPage = 1;
    private const int DefaultPageSize = 20;

    /// <summary>
    /// Đăng ký các endpoint Thông báo trong ứng dụng vào route builder.
    /// </summary>
    /// <param name="endpoints">Route builder của ứng dụng.</param>
    /// <returns>Route builder để chuỗi cấu hình.</returns>
    public static IEndpointRouteBuilder MapDeliveryEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        endpoints.MapGet("/api/in-app-notifications/unread-count", HandleGetUnreadCountAsync)
            .WithTags("InAppNotification")
            .RequireAuthorization();

        endpoints.MapGet("/api/in-app-notifications", HandleGetInAppNotificationsAsync)
            .WithTags("InAppNotification")
            .RequireAuthorization();

        endpoints.MapGet("/api/in-app-notifications/{id:guid}", HandleGetInAppNotificationByIdAsync)
            .WithTags("InAppNotification")
            .RequireAuthorization();

        endpoints.MapPut("/api/in-app-notifications/{id:guid}/read", HandleMarkInAppNotificationAsReadAsync)
            .WithTags("InAppNotification")
            .RequireAuthorization();

        endpoints.MapGet("/api/notifications/{notificationId:guid}/delivery-history", HandleGetNotificationDeliveryHistoryAsync)
            .WithTags("NotificationDeliveryHistory")
            .RequireAuthorization(RbacPolicies.NotificationRead);

        return endpoints;
    }

    /// <summary>
    /// Xử lý yêu cầu lấy danh sách Thông báo trong ứng dụng.
    /// </summary>
    /// <param name="user">Thông tin người dùng hiện tại.</param>
    /// <param name="useCase">Use case lấy danh sách Thông báo trong ứng dụng.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <param name="unreadOnly">Chỉ lấy Thông báo chưa đọc.</param>
    /// <param name="page">Số trang.</param>
    /// <param name="pageSize">Kích thước trang.</param>
    /// <returns>Kết quả HTTP của truy vấn danh sách.</returns>
    private static async Task<IResult> HandleGetInAppNotificationsAsync(
        ClaimsPrincipal user,
        GetInAppNotificationsUseCase useCase,
        CancellationToken cancellationToken,
        bool unreadOnly = false,
        int page = DefaultPage,
        int pageSize = DefaultPageSize)
    {
        var query = new GetInAppNotificationsQuery(GetRecipientUserId(user), unreadOnly, page, pageSize);
        var result = await useCase.HandleAsync(query, cancellationToken);
        return ToHttpResult(result);
    }

    /// <summary>
    /// Xử lý yêu cầu lấy chi tiết Thông báo trong ứng dụng.
    /// </summary>
    /// <param name="id">Mã định danh Thông báo trong ứng dụng.</param>
    /// <param name="user">Thông tin người dùng hiện tại.</param>
    /// <param name="useCase">Use case lấy chi tiết Thông báo trong ứng dụng.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả HTTP của truy vấn chi tiết.</returns>
    private static async Task<IResult> HandleGetInAppNotificationByIdAsync(
        Guid id,
        ClaimsPrincipal user,
        GetInAppNotificationByIdUseCase useCase,
        CancellationToken cancellationToken)
    {
        var query = new GetInAppNotificationByIdQuery(id, GetRecipientUserId(user));
        var result = await useCase.HandleAsync(query, cancellationToken);
        return ToHttpResult(result);
    }

    /// <summary>
    /// Xử lý yêu cầu lấy số lượng Thông báo trong ứng dụng chưa đọc.
    /// </summary>
    /// <param name="user">Thông tin người dùng hiện tại.</param>
    /// <param name="useCase">Use case lấy số lượng chưa đọc.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả HTTP của truy vấn số lượng chưa đọc.</returns>
    private static async Task<IResult> HandleGetUnreadCountAsync(
        ClaimsPrincipal user,
        GetUnreadInAppNotificationCountUseCase useCase,
        CancellationToken cancellationToken)
    {
        var query = new GetUnreadInAppNotificationCountQuery(GetRecipientUserId(user));
        var result = await useCase.HandleAsync(query, cancellationToken);
        return ToHttpResult(result);
    }

    /// <summary>
    /// Xử lý yêu cầu đánh dấu Thông báo trong ứng dụng là đã đọc.
    /// </summary>
    /// <param name="id">Mã định danh Thông báo trong ứng dụng.</param>
    /// <param name="user">Thông tin người dùng hiện tại.</param>
    /// <param name="useCase">Use case đánh dấu đã đọc.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả HTTP của thao tác đánh dấu đã đọc.</returns>
    private static async Task<IResult> HandleMarkInAppNotificationAsReadAsync(
        Guid id,
        ClaimsPrincipal user,
        MarkInAppNotificationAsReadUseCase useCase,
        CancellationToken cancellationToken)
    {
        var command = new MarkInAppNotificationAsReadCommand(id, GetRecipientUserId(user));
        var result = await useCase.HandleAsync(command, cancellationToken);
        return ToHttpResult(result);
    }

    /// <summary>
    /// Xử lý yêu cầu lấy lịch sử gửi Thông báo.
    /// </summary>
    /// <param name="notificationId">Mã định danh Thông báo.</param>
    /// <param name="useCase">Use case lấy lịch sử gửi Thông báo.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <param name="channel">Kênh gửi cần lọc.</param>
    /// <param name="result">Kết quả cần lọc.</param>
    /// <param name="page">Số trang.</param>
    /// <param name="pageSize">Kích thước trang.</param>
    /// <returns>Kết quả HTTP của truy vấn lịch sử gửi.</returns>
    private static async Task<IResult> HandleGetNotificationDeliveryHistoryAsync(
        Guid notificationId,
        GetNotificationDeliveryHistoryUseCase useCase,
        CancellationToken cancellationToken,
        string? channel = null,
        string? result = null,
        int page = DefaultPage,
        int pageSize = DefaultPageSize)
    {
        var query = new GetNotificationDeliveryHistoryQuery(notificationId, channel, result, page, pageSize);
        var resultData = await useCase.HandleAsync(query, cancellationToken);
        return ToHttpResult(resultData);
    }

    /// <summary>
    /// Lấy mã định danh Người dùng cuối từ claims.
    /// </summary>
    /// <param name="user">Thông tin người dùng hiện tại.</param>
    /// <returns>Mã định danh Người dùng cuối nếu có; ngược lại null.</returns>
    private static string? GetRecipientUserId(ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user.FindFirstValue("sub");
    }

    /// <summary>
    /// Chuyển kết quả tầng Application sang kết quả HTTP.
    /// </summary>
    /// <typeparam name="T">Kiểu dữ liệu trả về.</typeparam>
    /// <param name="result">Kết quả xử lý từ tầng Application.</param>
    /// <returns>Kết quả HTTP tương ứng.</returns>
    private static IResult ToHttpResult<T>(InAppNotificationAppResult<T> result)
    {
        return result.Status switch
        {
            InAppNotificationAppStatus.Ok => Results.Ok(result.Value),
            InAppNotificationAppStatus.Created => Results.Ok(result.Value),
            InAppNotificationAppStatus.ValidationError => Results.ValidationProblem(result.ValidationErrors ?? new Dictionary<string, string[]>()),
            InAppNotificationAppStatus.NotFound => Results.NotFound(),
            InAppNotificationAppStatus.Forbidden => Results.Forbid(),
            _ => Results.Problem("Unexpected application result status.")
        };
    }

    /// <summary>
    /// Chuyển kết quả tầng Application lịch sử gửi sang kết quả HTTP.
    /// </summary>
    /// <typeparam name="T">Kiểu dữ liệu trả về.</typeparam>
    /// <param name="result">Kết quả xử lý từ tầng Application.</param>
    /// <returns>Kết quả HTTP tương ứng.</returns>
    private static IResult ToHttpResult<T>(DeliveryHistoryAppResult<T> result)
    {
        return result.Status switch
        {
            DeliveryHistoryAppStatus.Ok => Results.Ok(result.Value),
            DeliveryHistoryAppStatus.ValidationError => Results.ValidationProblem(result.ValidationErrors ?? new Dictionary<string, string[]>()),
            DeliveryHistoryAppStatus.NotFound => Results.NotFound(),
            DeliveryHistoryAppStatus.Forbidden => Results.Forbid(),
            _ => Results.Problem("Unexpected application result status.")
        };
    }
}
