using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Observability.Api;

/// <summary>
/// Cung cấp helper tạo phản hồi ProblemDetails chuẩn cho API.
/// </summary>
public static class ApiProblemResults
{
    /// <summary>
    /// Mã lỗi nghiệp vụ cho yêu cầu không hợp lệ.
    /// </summary>
    public const string ValidationErrorCode = "validation_error";

    /// <summary>
    /// Mã lỗi nghiệp vụ cho tài nguyên không tồn tại.
    /// </summary>
    public const string NotFoundCode = "not_found";

    /// <summary>
    /// Mã lỗi nghiệp vụ cho xung đột dữ liệu.
    /// </summary>
    public const string ConflictCode = "conflict";

    /// <summary>
    /// Mã lỗi nghiệp vụ cho yêu cầu chưa xác thực.
    /// </summary>
    public const string UnauthorizedCode = "unauthorized";

    /// <summary>
    /// Mã lỗi nghiệp vụ cho yêu cầu bị từ chối quyền truy cập.
    /// </summary>
    public const string ForbiddenCode = "forbidden";

    /// <summary>
    /// Mã lỗi nghiệp vụ cho lỗi hệ thống không mong đợi.
    /// </summary>
    public const string UnexpectedErrorCode = "unexpected_error";

    /// <summary>
    /// Tạo phản hồi ProblemDetails.
    /// </summary>
    /// <param name="statusCode">Mã trạng thái HTTP.</param>
    /// <param name="title">Tiêu đề lỗi.</param>
    /// <param name="detail">Chi tiết lỗi.</param>
    /// <param name="code">Mã lỗi nghiệp vụ.</param>
    /// <param name="errors">Danh sách lỗi validation.</param>
    /// <returns>Kết quả HTTP ProblemDetails.</returns>
    public static IResult Problem(
        int statusCode,
        string title,
        string detail,
        string code,
        IDictionary<string, string[]>? errors = null)
    {
        var extensions = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["code"] = code
        };

        if (errors is not null)
        {
            return Results.ValidationProblem(
                errors,
                detail: detail,
                title: title,
                statusCode: statusCode,
                extensions: extensions);
        }

        return Results.Problem(
            title: title,
            detail: detail,
            statusCode: statusCode,
            extensions: extensions);
    }
}
