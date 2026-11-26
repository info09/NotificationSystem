using Administration.Application.Abstractions;
using Administration.Application.Dashboard;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Api.Modules.Dashboard;

/// <summary>
/// Bộ điều hợp đọc tổng hợp Kiểm tra sức khỏe cho Tổng quan.
/// </summary>
public sealed class DashboardHealthSummaryReaderAdapter : IDashboardHealthSummaryReader
{
    private readonly HealthCheckService _healthCheckService;

    /// <summary>
    /// Khởi tạo bộ điều hợp đọc tổng hợp Kiểm tra sức khỏe.
    /// </summary>
    /// <param name="healthCheckService">Dịch vụ kiểm tra sức khỏe.</param>
    public DashboardHealthSummaryReaderAdapter(HealthCheckService healthCheckService)
    {
        _healthCheckService = healthCheckService;
    }

    /// <summary>
    /// Lấy tổng hợp trạng thái Kiểm tra sức khỏe.
    /// </summary>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Tổng hợp Kiểm tra sức khỏe.</returns>
    public async Task<HealthSummaryDto> GetSummaryAsync(CancellationToken cancellationToken)
    {
        var report = await _healthCheckService.CheckHealthAsync(cancellationToken);
        var details = string.Join("; ", report.Entries.Select(entry => $"{entry.Key}:{entry.Value.Status}"));
        return new HealthSummaryDto(report.Status.ToString(), string.IsNullOrWhiteSpace(details) ? null : details);
    }
}
