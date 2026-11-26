using Administration.Application.Abstractions;
using Notification.Application.Notifications;

namespace Api.Modules.Dashboard;

/// <summary>
/// Bộ điều hợp đọc tổng hợp Trạng thái thông báo cho Tổng quan.
/// </summary>
public sealed class NotificationStatusSummaryReaderAdapter : INotificationStatusSummaryReader
{
    private readonly INotificationRepository _notificationRepository;

    /// <summary>
    /// Khởi tạo bộ điều hợp đọc tổng hợp Trạng thái thông báo.
    /// </summary>
    /// <param name="notificationRepository">Repository Thông báo.</param>
    public NotificationStatusSummaryReaderAdapter(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    /// <summary>
    /// Lấy tổng hợp số lượng Thông báo theo Trạng thái thông báo.
    /// </summary>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Bản đồ số lượng theo trạng thái.</returns>
    public async Task<IReadOnlyDictionary<string, int>> GetStatusCountsAsync(CancellationToken cancellationToken)
    {
        var counts = await _notificationRepository.GetStatusCountsAsync(scopeRestriction: null, cancellationToken);
        return counts.ToDictionary(x => x.Key.ToString(), x => x.Value, StringComparer.Ordinal);
    }
}
