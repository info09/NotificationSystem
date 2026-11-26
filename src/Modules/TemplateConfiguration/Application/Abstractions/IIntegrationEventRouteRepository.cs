using TemplateConfiguration.Domain.IntegrationEventRoutes;

namespace TemplateConfiguration.Application.Abstractions;

/// <summary>
/// Repository truy cập dữ liệu Định tuyến sự kiện tích hợp.
/// </summary>
public interface IIntegrationEventRouteRepository
{
    /// <summary>
    /// Lấy Định tuyến sự kiện tích hợp theo mã định danh.
    /// </summary>
    /// <param name="id">Mã định danh định tuyến.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Định tuyến hoặc <c>null</c>.</returns>
    Task<IntegrationEventRoute?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Lấy danh sách Định tuyến sự kiện tích hợp có phân trang.
    /// </summary>
    /// <param name="eventType">Loại sự kiện lọc.</param>
    /// <param name="channel">Kênh lọc.</param>
    /// <param name="isActive">Trạng thái kích hoạt lọc.</param>
    /// <param name="page">Số trang.</param>
    /// <param name="pageSize">Kích thước trang.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Danh sách định tuyến và tổng số bản ghi.</returns>
    Task<(IReadOnlyList<IntegrationEventRoute> Items, int TotalCount)> GetPagedAsync(
        string? eventType,
        string? channel,
        bool? isActive,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    /// <summary>
    /// Kiểm tra tồn tại định tuyến theo loại sự kiện.
    /// </summary>
    /// <param name="eventType">Loại sự kiện tích hợp.</param>
    /// <param name="excludeId">Mã định danh cần loại trừ.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns><c>true</c> nếu tồn tại; ngược lại <c>false</c>.</returns>
    Task<bool> ExistsByEventTypeAsync(string eventType, Guid? excludeId, CancellationToken cancellationToken);

    /// <summary>
    /// Lấy Định tuyến sự kiện tích hợp đang hoạt động theo loại sự kiện.
    /// </summary>
    /// <param name="eventType">Loại sự kiện tích hợp.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Định tuyến hoạt động hoặc <c>null</c>.</returns>
    Task<IntegrationEventRoute?> GetActiveByEventTypeAsync(string eventType, CancellationToken cancellationToken);

    /// <summary>
    /// Thêm Định tuyến sự kiện tích hợp.
    /// </summary>
    /// <param name="route">Thực thể định tuyến.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    Task AddAsync(IntegrationEventRoute route, CancellationToken cancellationToken);

    /// <summary>
    /// Xóa Định tuyến sự kiện tích hợp.
    /// </summary>
    /// <param name="route">Thực thể định tuyến.</param>
    void Remove(IntegrationEventRoute route);

    /// <summary>
    /// Lưu thay đổi dữ liệu.
    /// </summary>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
