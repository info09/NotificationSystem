using BuildingBlocks.SharedKernel.Constants;
using IdentityAccess.Application;
using IdentityAccess.Application.UserAccounts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

namespace IdentityAccess.Presentation;

/// <summary>
/// Phương thức mở rộng ánh xạ endpoint cho module IdentityAccess.
/// </summary>
public static class IdentityAccessEndpointRouteBuilderExtensions
{
    /// <summary>
    /// Đăng ký các endpoint IdentityAccess vào route builder.
    /// </summary>
    /// <param name="endpoints">Route builder của ứng dụng.</param>
    /// <returns>Route builder để chuỗi cấu hình.</returns>
    public static IEndpointRouteBuilder MapIdentityAccessEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var group = endpoints.MapGroup("/api/admin/users")
            .WithTags("IdentityAccess")
            .RequireAuthorization(RbacPolicies.SystemAdministrator);

        group.MapGet("/", HandleGetUserAccountsAsync);
        group.MapGet("/{id:guid}", HandleGetUserAccountByIdAsync);
        group.MapPost("/", HandleCreateUserAccountAsync);
        group.MapPut("/{id:guid}", HandleUpdateUserAccountAsync);
        group.MapPost("/{id:guid}/lock", HandleLockUserAccountAsync);
        group.MapPost("/{id:guid}/unlock", HandleUnlockUserAccountAsync);
        group.MapDelete("/{id:guid}", HandleDeleteUserAccountAsync);
        group.MapGet("/{id:guid}/credentials", HandleGetCredentialsAsync);
        group.MapPost("/{id:guid}/credentials", HandleCreateCredentialAsync);
        group.MapPost("/{id:guid}/credentials/{credentialId:guid}/rotate", HandleRotateCredentialAsync);
        group.MapPost("/{id:guid}/credentials/{credentialId:guid}/revoke", HandleRevokeCredentialAsync);

        return endpoints;
    }

    /// <summary>
    /// Xử lý yêu cầu truy vấn danh sách Tài khoản người dùng.
    /// </summary>
    /// <param name="useCase">Use case truy vấn danh sách.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <param name="status">Trạng thái lọc.</param>
    /// <param name="role">Vai trò lọc.</param>
    /// <param name="page">Số trang.</param>
    /// <param name="pageSize">Kích thước trang.</param>
    /// <returns>Kết quả HTTP.</returns>
    private static async Task<IResult> HandleGetUserAccountsAsync(
        GetUserAccountsUseCase useCase,
        CancellationToken cancellationToken,
        string? status = null,
        string? role = null,
        int page = 1,
        int pageSize = 20)
    {
        var result = await useCase.HandleAsync(new GetUserAccountsAppRequest(status, role, page, pageSize), cancellationToken);
        return ToHttpResult(result);
    }

    /// <summary>
    /// Xử lý yêu cầu lấy chi tiết Tài khoản người dùng.
    /// </summary>
    /// <param name="id">Mã định danh tài khoản.</param>
    /// <param name="useCase">Use case lấy chi tiết.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả HTTP.</returns>
    private static async Task<IResult> HandleGetUserAccountByIdAsync(
        Guid id,
        GetUserAccountByIdUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.HandleAsync(id, cancellationToken);
        return ToHttpResult(result);
    }

    /// <summary>
    /// Xử lý yêu cầu tạo Tài khoản người dùng.
    /// </summary>
    /// <param name="request">Yêu cầu tạo tài khoản.</param>
    /// <param name="useCase">Use case tạo tài khoản.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả HTTP.</returns>
    private static async Task<IResult> HandleCreateUserAccountAsync(
        CreateUserAccountRequest? request,
        CreateUserAccountUseCase useCase,
        CancellationToken cancellationToken)
    {
        var appRequest = request is null
            ? null
            : new CreateUserAccountAppRequest(request.UserName, request.Email, request.DisplayName, request.Role, request.Password);
        var result = await useCase.HandleAsync(appRequest, cancellationToken);
        return ToHttpResult(result, value => $"/api/admin/users/{value.Id}");
    }

    /// <summary>
    /// Xử lý yêu cầu cập nhật Tài khoản người dùng.
    /// </summary>
    /// <param name="id">Mã định danh tài khoản.</param>
    /// <param name="request">Yêu cầu cập nhật.</param>
    /// <param name="useCase">Use case cập nhật tài khoản.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả HTTP.</returns>
    private static async Task<IResult> HandleUpdateUserAccountAsync(
        Guid id,
        UpdateUserAccountRequest? request,
        UpdateUserAccountUseCase useCase,
        CancellationToken cancellationToken)
    {
        var appRequest = request is null
            ? null
            : new UpdateUserAccountAppRequest(request.UserName, request.Email, request.DisplayName, request.Role, request.Password);
        var result = await useCase.HandleAsync(id, appRequest, cancellationToken);
        return ToHttpResult(result);
    }

    /// <summary>
    /// Xử lý yêu cầu khóa Tài khoản người dùng.
    /// </summary>
    /// <param name="id">Mã định danh tài khoản.</param>
    /// <param name="user">Thông tin người dùng hiện tại.</param>
    /// <param name="useCase">Use case khóa tài khoản.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả HTTP.</returns>
    private static async Task<IResult> HandleLockUserAccountAsync(
        Guid id,
        ClaimsPrincipal user,
        LockUserAccountUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.HandleAsync(id, CreateCallerContext(user), cancellationToken);
        return ToHttpResult(result);
    }

    /// <summary>
    /// Xử lý yêu cầu mở khóa Tài khoản người dùng.
    /// </summary>
    /// <param name="id">Mã định danh tài khoản.</param>
    /// <param name="useCase">Use case mở khóa tài khoản.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả HTTP.</returns>
    private static async Task<IResult> HandleUnlockUserAccountAsync(
        Guid id,
        UnlockUserAccountUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.HandleAsync(id, cancellationToken);
        return ToHttpResult(result);
    }

    /// <summary>
    /// Xử lý yêu cầu xóa Tài khoản người dùng.
    /// </summary>
    /// <param name="id">Mã định danh tài khoản.</param>
    /// <param name="user">Thông tin người dùng hiện tại.</param>
    /// <param name="useCase">Use case xóa tài khoản.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả HTTP.</returns>
    private static async Task<IResult> HandleDeleteUserAccountAsync(
        Guid id,
        ClaimsPrincipal user,
        DeleteUserAccountUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.HandleAsync(id, CreateCallerContext(user), cancellationToken);
        return ToHttpResult(result);
    }

    /// <summary>
    /// Xử lý yêu cầu lấy danh sách credential của IntegrationClient.
    /// </summary>
    /// <param name="id">Mã định danh tài khoản.</param>
    /// <param name="useCase">Use case lấy danh sách credential.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả HTTP.</returns>
    private static async Task<IResult> HandleGetCredentialsAsync(
        Guid id,
        GetIntegrationClientCredentialsUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.HandleAsync(id, cancellationToken);
        return ToHttpResult(result);
    }

    /// <summary>
    /// Xử lý yêu cầu tạo credential cho IntegrationClient.
    /// </summary>
    /// <param name="id">Mã định danh tài khoản.</param>
    /// <param name="request">Yêu cầu tạo credential.</param>
    /// <param name="useCase">Use case tạo credential.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả HTTP.</returns>
    private static async Task<IResult> HandleCreateCredentialAsync(
        Guid id,
        CreateIntegrationClientCredentialRequest? request,
        CreateIntegrationClientCredentialUseCase useCase,
        CancellationToken cancellationToken)
    {
        var appRequest = request is null
            ? new CreateIntegrationClientCredentialAppRequest(null, null)
            : new CreateIntegrationClientCredentialAppRequest(request.ClientId, request.ExpiresAtUtc);
        var result = await useCase.HandleAsync(id, appRequest, cancellationToken);
        return ToHttpResult(result, value => $"/api/admin/users/{id}/credentials/{value.Credential.Id}");
    }

    /// <summary>
    /// Xử lý yêu cầu xoay secret credential.
    /// </summary>
    /// <param name="id">Mã định danh tài khoản.</param>
    /// <param name="credentialId">Mã định danh credential.</param>
    /// <param name="useCase">Use case xoay secret.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả HTTP.</returns>
    private static async Task<IResult> HandleRotateCredentialAsync(
        Guid id,
        Guid credentialId,
        RotateIntegrationClientCredentialUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.HandleAsync(id, credentialId, cancellationToken);
        return ToHttpResult(result);
    }

    /// <summary>
    /// Xử lý yêu cầu vô hiệu hóa credential.
    /// </summary>
    /// <param name="id">Mã định danh tài khoản.</param>
    /// <param name="credentialId">Mã định danh credential.</param>
    /// <param name="useCase">Use case vô hiệu hóa credential.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả HTTP.</returns>
    private static async Task<IResult> HandleRevokeCredentialAsync(
        Guid id,
        Guid credentialId,
        RevokeIntegrationClientCredentialUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.HandleAsync(id, credentialId, cancellationToken);
        return ToHttpResult(result);
    }

    /// <summary>
    /// Tạo ngữ cảnh người gọi từ claims.
    /// </summary>
    /// <param name="user">Thông tin người dùng hiện tại.</param>
    /// <returns>Ngữ cảnh người gọi.</returns>
    private static IdentityAccessCallerContext CreateCallerContext(ClaimsPrincipal user)
    {
        var subject = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
        var roles = user.Claims
            .Where(claim => claim.Type == ClaimTypes.Role)
            .Select(claim => claim.Value)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        return new IdentityAccessCallerContext(subject, roles);
    }

    /// <summary>
    /// Chuyển kết quả tầng Application sang kết quả HTTP.
    /// </summary>
    /// <typeparam name="T">Kiểu dữ liệu trả về.</typeparam>
    /// <param name="result">Kết quả xử lý.</param>
    /// <param name="createdLocationFactory">Hàm tạo đường dẫn tài nguyên khi tạo mới.</param>
    /// <returns>Kết quả HTTP tương ứng.</returns>
    private static IResult ToHttpResult<T>(IdentityAccessAppResult<T> result, Func<T, string>? createdLocationFactory = null)
    {
        return result.Status switch
        {
            IdentityAccessAppStatus.Ok => Results.Ok(result.Value),
            IdentityAccessAppStatus.Created => createdLocationFactory is not null && result.Value is not null
                ? Results.Created(createdLocationFactory(result.Value), result.Value)
                : Results.Ok(result.Value),
            IdentityAccessAppStatus.ValidationError => Results.ValidationProblem(result.ValidationErrors ?? new Dictionary<string, string[]>()),
            IdentityAccessAppStatus.NotFound => Results.NotFound(),
            IdentityAccessAppStatus.Conflict => Results.Conflict(),
            _ => Results.Problem("Unexpected application result status.")
        };
    }
}

/// <summary>
/// Yêu cầu tạo Tài khoản người dùng.
/// </summary>
/// <param name="UserName">Tên đăng nhập.</param>
/// <param name="Email">Email.</param>
/// <param name="DisplayName">Tên hiển thị.</param>
/// <param name="Role">Vai trò.</param>
/// <param name="Password">Mật khẩu plaintext.</param>
public sealed record CreateUserAccountRequest(
    string? UserName,
    string? Email,
    string? DisplayName,
    string? Role,
    string? Password);

/// <summary>
/// Yêu cầu cập nhật Tài khoản người dùng.
/// </summary>
/// <param name="UserName">Tên đăng nhập.</param>
/// <param name="Email">Email.</param>
/// <param name="DisplayName">Tên hiển thị.</param>
/// <param name="Role">Vai trò.</param>
/// <param name="Password">Mật khẩu plaintext mới.</param>
public sealed record UpdateUserAccountRequest(
    string? UserName,
    string? Email,
    string? DisplayName,
    string? Role,
    string? Password);

/// <summary>
/// Yêu cầu tạo Thông tin xác thực IntegrationClient.
/// </summary>
/// <param name="ClientId">ClientId tùy chọn.</param>
/// <param name="ExpiresAtUtc">Thời điểm hết hạn.</param>
public sealed record CreateIntegrationClientCredentialRequest(
    string? ClientId,
    DateTime? ExpiresAtUtc);
