using IntegrationMessaging.Domain;

namespace IntegrationMessaging.Application.Abstractions;

/// <summary>
/// Repository truy cập dữ liệu Sự kiện tích hợp.
/// </summary>
public interface IIntegrationEventRepository
{
    /// <summary>
    /// Lấy Sự kiện tích hợp theo MessageId.
    /// </summary>
    /// <param name="messageId">Mã định danh Thông điệp.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Sự kiện tích hợp hoặc <c>null</c>.</returns>
    Task<IntegrationEvent?> GetByMessageIdAsync(Guid messageId, CancellationToken cancellationToken);

    /// <summary>
    /// Thêm Sự kiện tích hợp.
    /// </summary>
    /// <param name="integrationEvent">Bản ghi Sự kiện tích hợp.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    Task AddAsync(IntegrationEvent integrationEvent, CancellationToken cancellationToken);

    /// <summary>
    /// Lưu thay đổi dữ liệu.
    /// </summary>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
