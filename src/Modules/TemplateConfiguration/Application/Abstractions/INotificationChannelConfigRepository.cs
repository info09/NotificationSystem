using TemplateConfiguration.Domain.NotificationChannelConfigs;

namespace TemplateConfiguration.Application.Abstractions;

/// <summary>
/// Repository truy cập dữ liệu Cấu hình kênh thông báo.
/// </summary>
public interface INotificationChannelConfigRepository
{
    /// <summary>
    /// Lấy Cấu hình kênh thông báo theo mã định danh.
    /// </summary>
    /// <param name="id">Mã định danh cấu hình.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Cấu hình hoặc <c>null</c>.</returns>
    Task<NotificationChannelConfig?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Lấy danh sách Cấu hình kênh thông báo có phân trang.
    /// </summary>
    /// <param name="channel">Kênh lọc.</param>
    /// <param name="isEnabled">Trạng thái bật/tắt lọc.</param>
    /// <param name="page">Số trang.</param>
    /// <param name="pageSize">Kích thước trang.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Danh sách cấu hình và tổng số bản ghi.</returns>
    Task<(IReadOnlyList<NotificationChannelConfig> Items, int TotalCount)> GetPagedAsync(
        string? channel,
        bool? isEnabled,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    /// <summary>
    /// Kiểm tra tồn tại cấu hình theo kênh.
    /// </summary>
    /// <param name="channel">Kênh thông báo.</param>
    /// <param name="excludeId">Mã định danh cần loại trừ.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns><c>true</c> nếu tồn tại; ngược lại <c>false</c>.</returns>
    Task<bool> ExistsByChannelAsync(string channel, Guid? excludeId, CancellationToken cancellationToken);

    /// <summary>
    /// Thêm Cấu hình kênh thông báo.
    /// </summary>
    /// <param name="config">Thực thể cấu hình.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    Task AddAsync(NotificationChannelConfig config, CancellationToken cancellationToken);

    /// <summary>
    /// Xóa Cấu hình kênh thông báo.
    /// </summary>
    /// <param name="config">Thực thể cấu hình.</param>
    void Remove(NotificationChannelConfig config);

    /// <summary>
    /// Lưu thay đổi dữ liệu.
    /// </summary>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
