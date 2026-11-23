namespace Delivery.Application.DeliveryLogs;

/// <summary>
/// Trạng thái kết quả xử lý ở tầng Application cho lịch sử gửi.
/// </summary>
public enum DeliveryHistoryAppStatus
{
    /// <summary>
    /// Xử lý thành công.
    /// </summary>
    Ok,

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
/// Kết quả xử lý chuẩn của tầng Application cho lịch sử gửi.
/// </summary>
/// <typeparam name="T">Kiểu dữ liệu trả về.</typeparam>
/// <param name="Status">Trạng thái kết quả.</param>
/// <param name="Value">Giá trị trả về khi thành công.</param>
/// <param name="ValidationErrors">Danh sách lỗi kiểm tra dữ liệu.</param>
public sealed record DeliveryHistoryAppResult<T>(
    DeliveryHistoryAppStatus Status,
    T? Value,
    IReadOnlyDictionary<string, string[]>? ValidationErrors)
{
    /// <summary>
    /// Tạo kết quả thành công.
    /// </summary>
    /// <param name="value">Giá trị trả về.</param>
    /// <returns>Kết quả thành công.</returns>
    public static DeliveryHistoryAppResult<T> Ok(T value) => new(DeliveryHistoryAppStatus.Ok, value, null);

    /// <summary>
    /// Tạo kết quả lỗi kiểm tra dữ liệu.
    /// </summary>
    /// <param name="errors">Danh sách lỗi kiểm tra dữ liệu.</param>
    /// <returns>Kết quả lỗi kiểm tra dữ liệu.</returns>
    public static DeliveryHistoryAppResult<T> Validation(IReadOnlyDictionary<string, string[]> errors) =>
        new(DeliveryHistoryAppStatus.ValidationError, default, errors);

    /// <summary>
    /// Tạo kết quả không tìm thấy.
    /// </summary>
    /// <returns>Kết quả không tìm thấy.</returns>
    public static DeliveryHistoryAppResult<T> NotFound() => new(DeliveryHistoryAppStatus.NotFound, default, null);

    /// <summary>
    /// Tạo kết quả không có quyền truy cập.
    /// </summary>
    /// <returns>Kết quả không có quyền truy cập.</returns>
    public static DeliveryHistoryAppResult<T> Forbidden() => new(DeliveryHistoryAppStatus.Forbidden, default, null);
}
