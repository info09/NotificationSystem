namespace IdentityAccess.Application;

/// <summary>
/// Trạng thái kết quả xử lý ở tầng Application của IdentityAccess.
/// </summary>
public enum IdentityAccessAppStatus
{
    /// <summary>
    /// Xử lý thành công.
    /// </summary>
    Ok,

    /// <summary>
    /// Tạo mới thành công.
    /// </summary>
    Created,

    /// <summary>
    /// Dữ liệu không hợp lệ.
    /// </summary>
    ValidationError,

    /// <summary>
    /// Không tìm thấy dữ liệu.
    /// </summary>
    NotFound,

    /// <summary>
    /// Xung đột dữ liệu.
    /// </summary>
    Conflict
}

/// <summary>
/// Kết quả xử lý chuẩn của tầng Application cho IdentityAccess.
/// </summary>
/// <typeparam name="T">Kiểu dữ liệu trả về.</typeparam>
/// <param name="Status">Trạng thái kết quả.</param>
/// <param name="Value">Giá trị trả về khi thành công.</param>
/// <param name="ValidationErrors">Danh sách lỗi kiểm tra dữ liệu.</param>
public sealed record IdentityAccessAppResult<T>(
    IdentityAccessAppStatus Status,
    T? Value,
    IReadOnlyDictionary<string, string[]>? ValidationErrors)
{
    /// <summary>
    /// Tạo kết quả thành công.
    /// </summary>
    /// <param name="value">Giá trị trả về.</param>
    /// <returns>Kết quả thành công.</returns>
    public static IdentityAccessAppResult<T> Ok(T value) =>
        new(IdentityAccessAppStatus.Ok, value, null);

    /// <summary>
    /// Tạo kết quả tạo mới thành công.
    /// </summary>
    /// <param name="value">Giá trị trả về.</param>
    /// <returns>Kết quả tạo mới thành công.</returns>
    public static IdentityAccessAppResult<T> Created(T value) =>
        new(IdentityAccessAppStatus.Created, value, null);

    /// <summary>
    /// Tạo kết quả lỗi kiểm tra dữ liệu.
    /// </summary>
    /// <param name="errors">Danh sách lỗi theo trường.</param>
    /// <returns>Kết quả lỗi kiểm tra dữ liệu.</returns>
    public static IdentityAccessAppResult<T> Validation(IReadOnlyDictionary<string, string[]> errors) =>
        new(IdentityAccessAppStatus.ValidationError, default, errors);

    /// <summary>
    /// Tạo kết quả không tìm thấy dữ liệu.
    /// </summary>
    /// <returns>Kết quả không tìm thấy.</returns>
    public static IdentityAccessAppResult<T> NotFound() =>
        new(IdentityAccessAppStatus.NotFound, default, null);

    /// <summary>
    /// Tạo kết quả xung đột dữ liệu.
    /// </summary>
    /// <returns>Kết quả xung đột dữ liệu.</returns>
    public static IdentityAccessAppResult<T> Conflict() =>
        new(IdentityAccessAppStatus.Conflict, default, null);
}
