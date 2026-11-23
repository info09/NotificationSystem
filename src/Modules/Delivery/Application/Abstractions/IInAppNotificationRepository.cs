using BuildingBlocks.SharedKernel.Repository;
using Delivery.Domain.InAppNotifications;

namespace Delivery.Application.Abstractions;

/// <summary>
/// Repository truy cập dữ liệu Thông báo trong ứng dụng.
/// </summary>
public interface IInAppNotificationRepository : IRepository<InAppNotification>
{
    /// <summary>
    /// Lấy danh sách Thông báo trong ứng dụng của Người dùng cuối có phân trang.
    /// </summary>
    /// <param name="recipientUserId">Mã định danh Người dùng cuối.</param>
    /// <param name="unreadOnly">Chỉ lấy Thông báo chưa đọc.</param>
    /// <param name="page">Số trang.</param>
    /// <param name="pageSize">Kích thước trang.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Danh sách Thông báo trong ứng dụng và tổng số bản ghi.</returns>
    Task<(IReadOnlyList<InAppNotification> Items, int TotalCount)> GetPagedAsync(
        string recipientUserId,
        bool unreadOnly,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    /// <summary>
    /// Lấy chi tiết Thông báo trong ứng dụng theo mã định danh và Người dùng cuối.
    /// </summary>
    /// <param name="id">Mã định danh Thông báo trong ứng dụng.</param>
    /// <param name="recipientUserId">Mã định danh Người dùng cuối.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Thông báo trong ứng dụng nếu tìm thấy; ngược lại null.</returns>
    Task<InAppNotification?> GetByIdForRecipientAsync(
        Guid id,
        string recipientUserId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Đếm số Thông báo trong ứng dụng chưa đọc của Người dùng cuối.
    /// </summary>
    /// <param name="recipientUserId">Mã định danh Người dùng cuối.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Số lượng Thông báo chưa đọc.</returns>
    Task<int> GetUnreadCountAsync(string recipientUserId, CancellationToken cancellationToken);

    /// <summary>
    /// Kiểm tra Thông báo trong ứng dụng đã tồn tại theo Thông báo gốc và Người nhận.
    /// </summary>
    /// <param name="notificationId">Mã định danh Thông báo gốc.</param>
    /// <param name="recipientUserId">Mã định danh Người dùng cuối.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Thông báo trong ứng dụng nếu đã tồn tại; ngược lại null.</returns>
    Task<InAppNotification?> FindByNotificationAndRecipientAsync(
        Guid notificationId,
        string recipientUserId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Lấy Thông báo trong ứng dụng được theo dõi theo mã định danh và Người dùng cuối.
    /// </summary>
    /// <param name="id">Mã định danh Thông báo trong ứng dụng.</param>
    /// <param name="recipientUserId">Mã định danh Người dùng cuối.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Thông báo trong ứng dụng nếu tìm thấy; ngược lại null.</returns>
    Task<InAppNotification?> GetTrackedByIdForRecipientAsync(
        Guid id,
        string recipientUserId,
        CancellationToken cancellationToken);
}
