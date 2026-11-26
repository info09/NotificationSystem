namespace Administration.Application.Abstractions;

/// <summary>
/// Cổng đọc tổng hợp Trạng thái thông báo cho Tổng quan.
/// </summary>
public interface INotificationStatusSummaryReader
{
    /// <summary>
    /// Lấy tổng hợp số lượng Thông báo theo Trạng thái thông báo.
    /// </summary>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Bản đồ số lượng theo trạng thái.</returns>
    Task<IReadOnlyDictionary<string, int>> GetStatusCountsAsync(CancellationToken cancellationToken);
}