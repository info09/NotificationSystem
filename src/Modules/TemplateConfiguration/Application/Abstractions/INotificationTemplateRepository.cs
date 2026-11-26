using TemplateConfiguration.Domain.NotificationTemplates;

namespace TemplateConfiguration.Application.Abstractions;

/// <summary>
/// Repository truy cập dữ liệu Mẫu thông báo.
/// </summary>
public interface INotificationTemplateRepository
{
    /// <summary>
    /// Lấy Mẫu thông báo theo mã định danh.
    /// </summary>
    /// <param name="id">Mã định danh mẫu.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Mẫu thông báo hoặc <c>null</c>.</returns>
    Task<NotificationTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Lấy danh sách Mẫu thông báo có phân trang.
    /// </summary>
    /// <param name="channel">Kênh lọc.</param>
    /// <param name="isActive">Trạng thái kích hoạt lọc.</param>
    /// <param name="keyword">Từ khóa tìm kiếm.</param>
    /// <param name="page">Số trang.</param>
    /// <param name="pageSize">Kích thước trang.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Danh sách mẫu và tổng số bản ghi.</returns>
    Task<(IReadOnlyList<NotificationTemplate> Items, int TotalCount)> GetPagedAsync(
        string? channel,
        bool? isActive,
        string? keyword,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    /// <summary>
    /// Kiểm tra tồn tại khóa Mẫu thông báo.
    /// </summary>
    /// <param name="templateKey">Khóa mẫu thông báo.</param>
    /// <param name="excludeId">Mã định danh cần loại trừ.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns><c>true</c> nếu tồn tại; ngược lại <c>false</c>.</returns>
    Task<bool> ExistsByTemplateKeyAsync(string templateKey, Guid? excludeId, CancellationToken cancellationToken);

    /// <summary>
    /// Thêm Mẫu thông báo.
    /// </summary>
    /// <param name="template">Thực thể mẫu thông báo.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    Task AddAsync(NotificationTemplate template, CancellationToken cancellationToken);

    /// <summary>
    /// Xóa Mẫu thông báo.
    /// </summary>
    /// <param name="template">Thực thể mẫu thông báo.</param>
    void Remove(NotificationTemplate template);

    /// <summary>
    /// Lưu thay đổi dữ liệu.
    /// </summary>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
