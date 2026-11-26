namespace BuildingBlocks.SharedKernel.IntegrationEvents;

/// <summary>
/// Cổng phân giải Định tuyến sự kiện tích hợp.
/// </summary>
public interface IIntegrationEventRouteResolver
{
    /// <summary>
    /// Phân giải định tuyến đang hoạt động theo loại sự kiện.
    /// </summary>
    /// <param name="eventType">Loại sự kiện tích hợp.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả phân giải hoặc <c>null</c> nếu không có định tuyến hoạt động.</returns>
    Task<IntegrationEventRouteResolution?> ResolveAsync(string eventType, CancellationToken cancellationToken);
}
