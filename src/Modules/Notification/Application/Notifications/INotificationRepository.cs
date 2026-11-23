using BuildingBlocks.SharedKernel.Repository;
using DomainNotification = global::Notification.Domain.Notification;
using NotificationChannel = global::Notification.Domain.NotificationChannel;
using NotificationStatus = global::Notification.Domain.NotificationStatus;

namespace Notification.Application.Notifications;

/// <summary>
/// Repository truy cập dữ liệu Thông báo.
/// </summary>
public interface INotificationRepository : IRepository<DomainNotification>
{
    /// <summary>
    /// Lấy danh sách Thông báo có phân trang và bộ lọc.
    /// </summary>
    /// <param name="status">Trạng thái cần lọc.</param>
    /// <param name="channel">Kênh gửi cần lọc.</param>
    /// <param name="createdFromUtc">Thời điểm tạo từ (UTC).</param>
    /// <param name="createdToUtc">Thời điểm tạo đến (UTC).</param>
    /// <param name="page">Số trang.</param>
    /// <param name="pageSize">Kích thước trang.</param>
    /// <param name="scopeRestriction">Giới hạn phạm vi truy cập.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Danh sách Thông báo và tổng số bản ghi.</returns>
    Task<(IReadOnlyList<DomainNotification> Items, int TotalCount)> GetPagedAsync(
        NotificationStatus? status,
        NotificationChannel? channel,
        DateTime? createdFromUtc,
        DateTime? createdToUtc,
        int page,
        int pageSize,
        string? scopeRestriction,
        CancellationToken cancellationToken);

    /// <summary>
    /// Lấy chi tiết Thông báo theo mã định danh và phạm vi truy cập.
    /// </summary>
    /// <param name="id">Mã định danh Thông báo.</param>
    /// <param name="scopeRestriction">Giới hạn phạm vi truy cập.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Thông báo nếu tìm thấy; ngược lại null.</returns>
    Task<DomainNotification?> GetByIdForScopeAsync(
        Guid id,
        string? scopeRestriction,
        CancellationToken cancellationToken);
}