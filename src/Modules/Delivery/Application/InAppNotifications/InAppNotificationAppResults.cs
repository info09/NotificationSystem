namespace Delivery.Application.InAppNotifications;

/// <summary>
/// Trạng thái kết quả xử lý ở tầng Application cho Thông báo trong ứng dụng.
/// </summary>
public enum InAppNotificationAppStatus
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
    /// Không có quyền truy cập.
    /// </summary>
    Forbidden
}

/// <summary>
/// Kết quả xử lý chuẩn của tầng Application cho Thông báo trong ứng dụng.
/// </summary>
/// <typeparam name="T">Kiểu dữ liệu trả về.</typeparam>
/// <param name="Status">Trạng thái kết quả.</param>
/// <param name="Value">Giá trị trả về khi thành công.</param>
/// <param name="ValidationErrors">Danh sách lỗi kiểm tra dữ liệu.</param>
public sealed record InAppNotificationAppResult<T>(
    InAppNotificationAppStatus Status,
    T? Value,
    IReadOnlyDictionary<string, string[]>? ValidationErrors)
{
    /// <summary>
    /// Tạo kết quả thành công.
    /// </summary>
    /// <param name="value">Giá trị trả về.</param>
    /// <returns>Kết quả thành công.</returns>
    public static InAppNotificationAppResult<T> Ok(T value) =>
        new(InAppNotificationAppStatus.Ok, value, null);

    /// <summary>
    /// Tạo kết quả tạo mới thành công.
    /// </summary>
    /// <param name="value">Giá trị trả về.</param>
    /// <returns>Kết quả tạo mới thành công.</returns>
    public static InAppNotificationAppResult<T> Created(T value) =>
        new(InAppNotificationAppStatus.Created, value, null);

    /// <summary>
    /// Tạo kết quả lỗi kiểm tra dữ liệu.
    /// </summary>
    /// <param name="errors">Danh sách lỗi theo trường.</param>
    /// <returns>Kết quả lỗi kiểm tra dữ liệu.</returns>
    public static InAppNotificationAppResult<T> Validation(IReadOnlyDictionary<string, string[]> errors) =>
        new(InAppNotificationAppStatus.ValidationError, default, errors);

    /// <summary>
    /// Tạo kết quả không tìm thấy dữ liệu.
    /// </summary>
    /// <returns>Kết quả không tìm thấy.</returns>
    public static InAppNotificationAppResult<T> NotFound() =>
        new(InAppNotificationAppStatus.NotFound, default, null);

    /// <summary>
    /// Tạo kết quả không có quyền truy cập.
    /// </summary>
    /// <returns>Kết quả không có quyền truy cập.</returns>
    public static InAppNotificationAppResult<T> Forbidden() =>
        new(InAppNotificationAppStatus.Forbidden, default, null);
}
