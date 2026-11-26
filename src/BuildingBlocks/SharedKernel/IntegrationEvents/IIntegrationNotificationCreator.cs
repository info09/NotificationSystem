namespace BuildingBlocks.SharedKernel.IntegrationEvents;

/// <summary>
/// Cổng tạo Thông báo từ Sự kiện tích hợp.
/// </summary>
public interface IIntegrationNotificationCreator
{
    /// <summary>
    /// Tạo Thông báo từ lệnh ánh xạ Sự kiện tích hợp.
    /// </summary>
    /// <param name="command">Lệnh tạo Thông báo.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả tạo Thông báo.</returns>
    Task<IntegrationNotificationCreationResult> CreateAsync(
        CreateIntegrationNotificationCommand command,
        CancellationToken cancellationToken);
}
