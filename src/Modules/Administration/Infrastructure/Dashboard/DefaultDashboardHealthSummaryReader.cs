using Administration.Application.Abstractions;
using Administration.Application.Dashboard;

namespace Administration.Infrastructure.Dashboard;

/// <summary>
/// Bộ đọc mặc định cho tổng hợp Kiểm tra sức khỏe khi host không cung cấp adapter.
/// </summary>
public sealed class DefaultDashboardHealthSummaryReader : IDashboardHealthSummaryReader
{
    /// <summary>
    /// Lấy tổng hợp trạng thái Kiểm tra sức khỏe.
    /// </summary>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Trạng thái không xác định.</returns>
    public Task<HealthSummaryDto> GetSummaryAsync(CancellationToken cancellationToken)
        => Task.FromResult(new HealthSummaryDto("Unknown", "Chưa có adapter Kiểm tra sức khỏe cho host hiện tại."));
}
