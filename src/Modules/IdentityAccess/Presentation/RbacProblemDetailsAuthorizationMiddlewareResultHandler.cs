using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;

namespace IdentityAccess.Presentation;

/// <summary>
/// Xử lý kết quả phân quyền và trả về ProblemDetails chuẩn cho lỗi 401/403.
/// </summary>
public sealed class RbacProblemDetailsAuthorizationMiddlewareResultHandler
    : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();

    /// <summary>
    /// Xử lý kết quả phân quyền cho request hiện tại.
    /// </summary>
    /// <param name="next">Delegate tiếp theo trong pipeline.</param>
    /// <param name="context">Ngữ cảnh HTTP hiện tại.</param>
    /// <param name="policy">Policy phân quyền được áp dụng.</param>
    /// <param name="authorizeResult">Kết quả đánh giá phân quyền.</param>
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

    /// <summary>
    /// Ghi phản hồi lỗi theo định dạng ProblemDetails.
    /// </summary>
    /// <param name="context">Ngữ cảnh HTTP hiện tại.</param>
    /// <param name="statusCode">Mã trạng thái HTTP.</param>
    /// <param name="title">Tiêu đề lỗi.</param>
    /// <param name="detail">Chi tiết lỗi.</param>
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
