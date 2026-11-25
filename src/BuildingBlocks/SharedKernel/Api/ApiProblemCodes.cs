namespace BuildingBlocks.SharedKernel.Api;

/// <summary>
/// Mã lỗi nghiệp vụ chuẩn cho phản hồi ProblemDetails.
/// </summary>
public static class ApiProblemCodes
{
    /// <summary>
    /// Mã lỗi cho dữ liệu không hợp lệ.
    /// </summary>
    public const string ValidationError = "validation_error";

    /// <summary>
    /// Mã lỗi cho tài nguyên không tồn tại.
    /// </summary>
    public const string NotFound = "not_found";

    /// <summary>
    /// Mã lỗi cho xung đột dữ liệu.
    /// </summary>
    public const string Conflict = "conflict";

    /// <summary>
    /// Mã lỗi cho yêu cầu chưa xác thực hoặc xác thực thất bại.
    /// </summary>
    public const string Unauthorized = "unauthorized";

    /// <summary>
    /// Mã lỗi cho yêu cầu bị từ chối quyền truy cập.
    /// </summary>
    public const string Forbidden = "forbidden";

    /// <summary>
    /// Mã lỗi cho lỗi hệ thống không mong đợi.
    /// </summary>
    public const string UnexpectedError = "unexpected_error";
}
