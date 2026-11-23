namespace Administration.Application;

/// <summary>
/// Trạng thái kết quả xử lý ở tầng Application của Administration.
/// </summary>
public enum AdministrationAppStatus
{
    /// <summary>
    /// Xử lý thành công.
    /// </summary>
    Ok,

    /// <summary>
    /// Tạo mới hoặc xử lý thành công có tạo tài nguyên.
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
    /// Xung đột do idempotency hoặc trạng thái trùng.
    /// </summary>
    Conflict
}

/// <summary>
/// Kết quả xử lý chuẩn của tầng Application cho Administration.
/// </summary>
/// <typeparam name="T">Kiểu dữ liệu trả về.</typeparam>
/// <param name="Status">Trạng thái kết quả.</param>
/// <param name="Value">Giá trị trả về khi thành công.</param>
/// <param name="ValidationErrors">Danh sách lỗi kiểm tra dữ liệu.</param>
public sealed record AdministrationAppResult<T>(
    AdministrationAppStatus Status,
    T? Value,
    IReadOnlyDictionary<string, string[]>? ValidationErrors)
{
    /// <summary>
    /// Tạo kết quả thành công.
    /// </summary>
    /// <param name="value">Giá trị trả về.</param>
    /// <returns>Kết quả thành công.</returns>
    public static AdministrationAppResult<T> Ok(T value) =>
        new(AdministrationAppStatus.Ok, value, null);

    /// <summary>
    /// Tạo kết quả tạo mới thành công.
    /// </summary>
    /// <param name="value">Giá trị trả về.</param>
    /// <returns>Kết quả tạo mới thành công.</returns>
    public static AdministrationAppResult<T> Created(T value) =>
        new(AdministrationAppStatus.Created, value, null);

    /// <summary>
    /// Tạo kết quả lỗi kiểm tra dữ liệu.
    /// </summary>
    /// <param name="errors">Danh sách lỗi theo trường.</param>
    /// <returns>Kết quả lỗi kiểm tra dữ liệu.</returns>
    public static AdministrationAppResult<T> Validation(IReadOnlyDictionary<string, string[]> errors) =>
        new(AdministrationAppStatus.ValidationError, default, errors);

    /// <summary>
    /// Tạo kết quả không tìm thấy dữ liệu.
    /// </summary>
    /// <returns>Kết quả không tìm thấy.</returns>
    public static AdministrationAppResult<T> NotFound() =>
        new(AdministrationAppStatus.NotFound, default, null);

    /// <summary>
    /// Tạo kết quả xung đột.
    /// </summary>
    /// <returns>Kết quả xung đột.</returns>
    public static AdministrationAppResult<T> Conflict() =>
        new(AdministrationAppStatus.Conflict, default, null);
}
