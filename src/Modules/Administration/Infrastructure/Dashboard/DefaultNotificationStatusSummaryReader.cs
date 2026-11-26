using Administration.Application.Abstractions;

namespace Administration.Infrastructure.Dashboard;

/// <summary>
/// Bộ đọc mặc định cho tổng hợp Trạng thái thông báo khi host không cung cấp adapter.
/// </summary>
public sealed class DefaultNotificationStatusSummaryReader : INotificationStatusSummaryReader
{
    /// <summary>
    /// Lấy tổng hợp số lượng Thông báo theo Trạng thái thông báo.
    /// </summary>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Bản đồ rỗng khi không có adapter.</returns>
    public Task<IReadOnlyDictionary<string, int>> GetStatusCountsAsync(CancellationToken cancellationToken)
        => Task.FromResult<IReadOnlyDictionary<string, int>>(new Dictionary<string, int>(StringComparer.Ordinal));
}
