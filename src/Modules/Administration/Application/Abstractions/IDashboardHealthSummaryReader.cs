using Administration.Application.Dashboard;

namespace Administration.Application.Abstractions;

/// <summary>
/// Cổng đọc tổng hợp Kiểm tra sức khỏe cho Tổng quan.
/// </summary>
public interface IDashboardHealthSummaryReader
{
    /// <summary>
    /// Lấy tổng hợp trạng thái Kiểm tra sức khỏe.
    /// </summary>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Tổng hợp Kiểm tra sức khỏe.</returns>
    Task<HealthSummaryDto> GetSummaryAsync(CancellationToken cancellationToken);
}