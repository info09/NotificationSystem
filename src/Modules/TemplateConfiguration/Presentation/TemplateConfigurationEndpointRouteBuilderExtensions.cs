using BuildingBlocks.SharedKernel.Api;
using BuildingBlocks.SharedKernel.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using TemplateConfiguration.Application;
using TemplateConfiguration.Application.NotificationChannelConfigs;
using TemplateConfiguration.Application.NotificationTemplates;

namespace TemplateConfiguration.Presentation;

/// <summary>
/// Phương thức mở rộng ánh xạ endpoint cho module TemplateConfiguration.
/// </summary>
public static class TemplateConfigurationEndpointRouteBuilderExtensions
{
    /// <summary>
    /// Đăng ký các endpoint TemplateConfiguration vào route builder.
    /// </summary>
    /// <param name="endpoints">Route builder của ứng dụng.</param>
    /// <returns>Route builder để chuỗi cấu hình.</returns>
    public static IEndpointRouteBuilder MapTemplateConfigurationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var group = endpoints.MapGroup("/api/admin/notification-templates")
            .WithTags("TemplateConfiguration")
            .RequireAuthorization(RbacPolicies.SystemAdministrator);

        group.MapGet("/", HandleGetNotificationTemplatesAsync);
        group.MapGet("/{id:guid}", HandleGetNotificationTemplateByIdAsync);
        group.MapPost("/", HandleCreateNotificationTemplateAsync);
        group.MapPut("/{id:guid}", HandleUpdateNotificationTemplateAsync);
        group.MapDelete("/{id:guid}", HandleDeleteNotificationTemplateAsync);

        var channelConfigGroup = endpoints.MapGroup("/api/admin/notification-channel-configs")
            .WithTags("TemplateConfiguration")
            .RequireAuthorization(RbacPolicies.SystemAdministrator);

        channelConfigGroup.MapGet("/", HandleGetNotificationChannelConfigsAsync);
        channelConfigGroup.MapGet("/{id:guid}", HandleGetNotificationChannelConfigByIdAsync);
        channelConfigGroup.MapPost("/", HandleCreateNotificationChannelConfigAsync);
        channelConfigGroup.MapPut("/{id:guid}", HandleUpdateNotificationChannelConfigAsync);
        channelConfigGroup.MapDelete("/{id:guid}", HandleDeleteNotificationChannelConfigAsync);

        return endpoints;
    }

    /// <summary>
    /// Xử lý yêu cầu truy vấn danh sách Mẫu thông báo.
    /// </summary>
    /// <param name="useCase">Use case truy vấn danh sách.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <param name="channel">Kênh lọc.</param>
    /// <param name="isActive">Trạng thái kích hoạt lọc.</param>
    /// <param name="keyword">Từ khóa tìm kiếm.</param>
    /// <param name="page">Số trang.</param>
    /// <param name="pageSize">Kích thước trang.</param>
    /// <returns>Kết quả HTTP.</returns>
    private static async Task<IResult> HandleGetNotificationTemplatesAsync(
        GetNotificationTemplatesUseCase useCase,
        CancellationToken cancellationToken,
        string? channel = null,
        bool? isActive = null,
        string? keyword = null,
        int page = 1,
        int pageSize = 20)
    {
        var result = await useCase.HandleAsync(
            new GetNotificationTemplatesAppRequest(channel, isActive, keyword, page, pageSize),
            cancellationToken);
        return ToHttpResult(result);
    }

    /// <summary>
    /// Xử lý yêu cầu lấy chi tiết Mẫu thông báo.
    /// </summary>
    /// <param name="id">Mã định danh mẫu.</param>
    /// <param name="useCase">Use case lấy chi tiết.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả HTTP.</returns>
    private static async Task<IResult> HandleGetNotificationTemplateByIdAsync(
        Guid id,
        GetNotificationTemplateByIdUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.HandleAsync(id, cancellationToken);
        return ToHttpResult(result);
    }

    /// <summary>
    /// Xử lý yêu cầu tạo Mẫu thông báo.
    /// </summary>
    /// <param name="request">Yêu cầu tạo mẫu.</param>
    /// <param name="useCase">Use case tạo mẫu.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả HTTP.</returns>
    private static async Task<IResult> HandleCreateNotificationTemplateAsync(
        CreateNotificationTemplateRequest? request,
        CreateNotificationTemplateUseCase useCase,
        CancellationToken cancellationToken)
    {
        var appRequest = request is null
            ? null
            : new CreateNotificationTemplateAppRequest(
                request.TemplateKey,
                request.Name,
                request.Description,
                request.Channel,
                request.Subject,
                request.Body,
                request.IsActive);

        var result = await useCase.HandleAsync(appRequest, cancellationToken);
        return ToHttpResult(result, value => $"/api/admin/notification-templates/{value.Id}");
    }

    /// <summary>
    /// Xử lý yêu cầu cập nhật Mẫu thông báo.
    /// </summary>
    /// <param name="id">Mã định danh mẫu.</param>
    /// <param name="request">Yêu cầu cập nhật.</param>
    /// <param name="useCase">Use case cập nhật mẫu.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả HTTP.</returns>
    private static async Task<IResult> HandleUpdateNotificationTemplateAsync(
        Guid id,
        UpdateNotificationTemplateRequest? request,
        UpdateNotificationTemplateUseCase useCase,
        CancellationToken cancellationToken)
    {
        var appRequest = request is null
            ? null
            : new UpdateNotificationTemplateAppRequest(
                request.Name,
                request.Description,
                request.Channel,
                request.Subject,
                request.Body,
                request.IsActive);

        var result = await useCase.HandleAsync(id, appRequest, cancellationToken);
        return ToHttpResult(result);
    }

    /// <summary>
    /// Xử lý yêu cầu xóa Mẫu thông báo.
    /// </summary>
    /// <param name="id">Mã định danh mẫu.</param>
    /// <param name="useCase">Use case xóa mẫu.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả HTTP.</returns>
    private static async Task<IResult> HandleDeleteNotificationTemplateAsync(
        Guid id,
        DeleteNotificationTemplateUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.HandleAsync(id, cancellationToken);
        return ToHttpResult(result);
    }

    /// <summary>
    /// Xử lý yêu cầu truy vấn danh sách Cấu hình kênh thông báo.
    /// </summary>
    /// <param name="useCase">Use case truy vấn danh sách.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <param name="channel">Kênh lọc.</param>
    /// <param name="isEnabled">Trạng thái bật/tắt lọc.</param>
    /// <param name="page">Số trang.</param>
    /// <param name="pageSize">Kích thước trang.</param>
    /// <returns>Kết quả HTTP.</returns>
    private static async Task<IResult> HandleGetNotificationChannelConfigsAsync(
        GetNotificationChannelConfigsUseCase useCase,
        CancellationToken cancellationToken,
        string? channel = null,
        bool? isEnabled = null,
        int page = 1,
        int pageSize = 20)
    {
        var result = await useCase.HandleAsync(
            new GetNotificationChannelConfigsAppRequest(channel, isEnabled, page, pageSize),
            cancellationToken);
        return ToHttpResult(result);
    }

    /// <summary>
    /// Xử lý yêu cầu lấy chi tiết Cấu hình kênh thông báo.
    /// </summary>
    /// <param name="id">Mã định danh cấu hình.</param>
    /// <param name="useCase">Use case lấy chi tiết.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả HTTP.</returns>
    private static async Task<IResult> HandleGetNotificationChannelConfigByIdAsync(
        Guid id,
        GetNotificationChannelConfigByIdUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.HandleAsync(id, cancellationToken);
        return ToHttpResult(result);
    }

    /// <summary>
    /// Xử lý yêu cầu tạo Cấu hình kênh thông báo.
    /// </summary>
    /// <param name="request">Yêu cầu tạo cấu hình.</param>
    /// <param name="useCase">Use case tạo cấu hình.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả HTTP.</returns>
    private static async Task<IResult> HandleCreateNotificationChannelConfigAsync(
        CreateNotificationChannelConfigRequest? request,
        CreateNotificationChannelConfigUseCase useCase,
        CancellationToken cancellationToken)
    {
        var appRequest = request is null
            ? null
            : new CreateNotificationChannelConfigAppRequest(
                request.Channel,
                request.DisplayName,
                request.Description,
                request.IsEnabled);

        var result = await useCase.HandleAsync(appRequest, cancellationToken);
        return ToHttpResult(result, value => $"/api/admin/notification-channel-configs/{value.Id}");
    }

    /// <summary>
    /// Xử lý yêu cầu cập nhật Cấu hình kênh thông báo.
    /// </summary>
    /// <param name="id">Mã định danh cấu hình.</param>
    /// <param name="request">Yêu cầu cập nhật.</param>
    /// <param name="useCase">Use case cập nhật cấu hình.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả HTTP.</returns>
    private static async Task<IResult> HandleUpdateNotificationChannelConfigAsync(
        Guid id,
        UpdateNotificationChannelConfigRequest? request,
        UpdateNotificationChannelConfigUseCase useCase,
        CancellationToken cancellationToken)
    {
        var appRequest = request is null
            ? null
            : new UpdateNotificationChannelConfigAppRequest(
                request.DisplayName,
                request.Description,
                request.IsEnabled);

        var result = await useCase.HandleAsync(id, appRequest, cancellationToken);
        return ToHttpResult(result);
    }

    /// <summary>
    /// Xử lý yêu cầu xóa Cấu hình kênh thông báo.
    /// </summary>
    /// <param name="id">Mã định danh cấu hình.</param>
    /// <param name="useCase">Use case xóa cấu hình.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả HTTP.</returns>
    private static async Task<IResult> HandleDeleteNotificationChannelConfigAsync(
        Guid id,
        DeleteNotificationChannelConfigUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.HandleAsync(id, cancellationToken);
        return ToHttpResult(result);
    }

    /// <summary>
    /// Chuyển kết quả tầng Application sang kết quả HTTP.
    /// </summary>
    /// <typeparam name="T">Kiểu dữ liệu trả về.</typeparam>
    /// <param name="result">Kết quả xử lý từ tầng Application.</param>
    /// <param name="createdLocationFactory">Hàm tạo đường dẫn tài nguyên mới.</param>
    /// <returns>Kết quả HTTP tương ứng.</returns>
    private static IResult ToHttpResult<T>(TemplateConfigurationAppResult<T> result, Func<T, string>? createdLocationFactory = null)
    {
        return result.Status switch
        {
            TemplateConfigurationAppStatus.Ok => Results.Ok(result.Value),
            TemplateConfigurationAppStatus.Created => createdLocationFactory is not null && result.Value is not null
                ? Results.Created(createdLocationFactory(result.Value), result.Value)
                : Results.Ok(result.Value),
            TemplateConfigurationAppStatus.ValidationError => Results.ValidationProblem(
                result.ValidationErrors ?? new Dictionary<string, string[]>(),
                title: "Validation Error",
                detail: "One or more validation errors occurred.",
                statusCode: StatusCodes.Status400BadRequest,
                extensions: new Dictionary<string, object?> { ["code"] = ApiProblemCodes.ValidationError }),
            TemplateConfigurationAppStatus.NotFound => Results.Problem(
                title: "Not Found",
                detail: "The requested resource was not found.",
                statusCode: StatusCodes.Status404NotFound,
                extensions: new Dictionary<string, object?> { ["code"] = ApiProblemCodes.NotFound }),
            TemplateConfigurationAppStatus.Conflict => Results.Problem(
                title: "Conflict",
                detail: "The request conflicts with the current state of the resource.",
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
/// Yêu cầu tạo Mẫu thông báo.
/// </summary>
/// <param name="TemplateKey">Khóa định danh nghiệp vụ.</param>
/// <param name="Name">Tên hiển thị.</param>
/// <param name="Description">Mô tả.</param>
/// <param name="Channel">Kênh thông báo.</param>
/// <param name="Subject">Tiêu đề.</param>
/// <param name="Body">Nội dung mẫu.</param>
/// <param name="IsActive">Trạng thái kích hoạt.</param>
public sealed record CreateNotificationTemplateRequest(
    string? TemplateKey,
    string? Name,
    string? Description,
    string? Channel,
    string? Subject,
    string? Body,
    bool? IsActive);

/// <summary>
/// Yêu cầu cập nhật Mẫu thông báo.
/// </summary>
/// <param name="Name">Tên hiển thị.</param>
/// <param name="Description">Mô tả.</param>
/// <param name="Channel">Kênh thông báo.</param>
/// <param name="Subject">Tiêu đề.</param>
/// <param name="Body">Nội dung mẫu.</param>
/// <param name="IsActive">Trạng thái kích hoạt.</param>
public sealed record UpdateNotificationTemplateRequest(
    string? Name,
    string? Description,
    string? Channel,
    string? Subject,
    string? Body,
    bool? IsActive);

/// <summary>
/// Yêu cầu tạo Cấu hình kênh thông báo.
/// </summary>
/// <param name="Channel">Kênh thông báo.</param>
/// <param name="DisplayName">Tên hiển thị.</param>
/// <param name="Description">Mô tả.</param>
/// <param name="IsEnabled">Trạng thái bật/tắt.</param>
public sealed record CreateNotificationChannelConfigRequest(
    string? Channel,
    string? DisplayName,
    string? Description,
    bool? IsEnabled);

/// <summary>
/// Yêu cầu cập nhật Cấu hình kênh thông báo.
/// </summary>
/// <param name="DisplayName">Tên hiển thị.</param>
/// <param name="Description">Mô tả.</param>
/// <param name="IsEnabled">Trạng thái bật/tắt.</param>
public sealed record UpdateNotificationChannelConfigRequest(
    string? DisplayName,
    string? Description,
    bool? IsEnabled);
