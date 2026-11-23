using Delivery.Domain.DeliveryLogs;

namespace Delivery.Application.Abstractions;

/// <summary>
/// Repository truy vấn lịch sử gửi Thông báo.
/// </summary>
public interface IDeliveryHistoryRepository
{
    /// <summary>
    /// Lấy danh sách Nhật ký gửi thông báo theo mã định danh Thông báo.
    /// </summary>
    /// <param name="notificationId">Mã định danh Thông báo.</param>
    /// <param name="fromUtc">Mốc thời gian sớm nhất (UTC).</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Danh sách Nhật ký gửi thông báo.</returns>
    Task<IReadOnlyList<NotificationDeliveryLog>> GetLogsByNotificationIdAsync(
        Guid notificationId,
        DateTime fromUtc,
        CancellationToken cancellationToken);

    /// <summary>
    /// Lấy danh sách Lần thử gửi theo mã định danh Thông báo có phân trang.
    /// </summary>
    /// <param name="notificationId">Mã định danh Thông báo.</param>
    /// <param name="fromUtc">Mốc thời gian sớm nhất (UTC).</param>
    /// <param name="channel">Kênh gửi cần lọc.</param>
    /// <param name="result">Kết quả cần lọc.</param>
    /// <param name="page">Số trang.</param>
    /// <param name="pageSize">Kích thước trang.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Danh sách Lần thử gửi và tổng số bản ghi.</returns>
    Task<(IReadOnlyList<DeliveryAttempt> Items, int TotalCount)> GetAttemptsByNotificationIdAsync(
        Guid notificationId,
        DateTime fromUtc,
        string? channel,
        string? result,
        int page,
        int pageSize,
        CancellationToken cancellationToken);
}
