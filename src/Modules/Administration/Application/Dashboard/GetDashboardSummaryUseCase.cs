namespace Administration.Application.Dashboard;

using Administration.Application.Abstractions;
using Administration.Application.DeadLetterQueue;

/// <summary>
/// Use case tổng hợp dữ liệu Tổng quan.
/// </summary>
public sealed class GetDashboardSummaryUseCase
{
    private static readonly string[] DeadLetterQueues =
    [
        "notifications.email.dlq",
        "notifications.inapp.dlq"
    ];

    private readonly INotificationStatusSummaryReader _notificationStatusSummaryReader;
    private readonly IDeadLetterQueueGateway _deadLetterQueueGateway;
    private readonly IDashboardHealthSummaryReader _dashboardHealthSummaryReader;

    /// <summary>
    /// Khởi tạo use case tổng hợp dữ liệu Tổng quan.
    /// </summary>
    /// <param name="notificationStatusSummaryReader">Cổng đọc tổng hợp Trạng thái thông báo.</param>
    /// <param name="deadLetterQueueGateway">Cổng truy cập Hàng đợi thư chết.</param>
    /// <param name="dashboardHealthSummaryReader">Cổng đọc tổng hợp Kiểm tra sức khỏe.</param>
    public GetDashboardSummaryUseCase(
        INotificationStatusSummaryReader notificationStatusSummaryReader,
        IDeadLetterQueueGateway deadLetterQueueGateway,
        IDashboardHealthSummaryReader dashboardHealthSummaryReader)
    {
        _notificationStatusSummaryReader = notificationStatusSummaryReader;
        _deadLetterQueueGateway = deadLetterQueueGateway;
        _dashboardHealthSummaryReader = dashboardHealthSummaryReader;
    }

    /// <summary>
    /// Xử lý truy vấn tổng hợp dữ liệu Tổng quan.
    /// </summary>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả tổng hợp dữ liệu Tổng quan.</returns>
    public async Task<AdministrationAppResult<DashboardSummaryResponse>> HandleAsync(CancellationToken cancellationToken)
    {
        var notificationsTask = GetNotificationSummaryAsync(cancellationToken);
        var deadLetterQueuesTask = GetDeadLetterQueueSummariesAsync(cancellationToken);
        var healthTask = GetHealthSummaryAsync(cancellationToken);

        await Task.WhenAll(notificationsTask, deadLetterQueuesTask, healthTask);

        var response = new DashboardSummaryResponse(
            await notificationsTask,
            await deadLetterQueuesTask,
            await healthTask);

        return AdministrationAppResult<DashboardSummaryResponse>.Ok(response);
    }

    /// <summary>
    /// Lấy tổng hợp Trạng thái thông báo.
    /// </summary>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Tổng hợp Trạng thái thông báo.</returns>
    private async Task<NotificationStatusSummaryDto> GetNotificationSummaryAsync(CancellationToken cancellationToken)
    {
        try
        {
            var counts = await _notificationStatusSummaryReader.GetStatusCountsAsync(cancellationToken);
            return new NotificationStatusSummaryDto(
                GetCount(counts, "Pending"),
                GetCount(counts, "Processing"),
                GetCount(counts, "Sent"),
                GetCount(counts, "Failed"));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            return new NotificationStatusSummaryDto(0, 0, 0, 0, "Không thể tải tổng hợp Trạng thái thông báo.");
        }
    }

    /// <summary>
    /// Lấy tổng hợp Hàng đợi thư chết.
    /// </summary>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Danh sách tổng hợp Hàng đợi thư chết.</returns>
    private async Task<IReadOnlyList<DeadLetterQueueSummaryDto>> GetDeadLetterQueueSummariesAsync(CancellationToken cancellationToken)
    {
        var tasks = DeadLetterQueues.Select(queueName => GetDeadLetterQueueSummaryAsync(queueName, cancellationToken));
        var results = await Task.WhenAll(tasks);
        return results;
    }

    /// <summary>
    /// Lấy tổng hợp một Hàng đợi thư chết.
    /// </summary>
    /// <param name="queueName">Tên Hàng đợi thư chết.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Tổng hợp Hàng đợi thư chết.</returns>
    private async Task<DeadLetterQueueSummaryDto> GetDeadLetterQueueSummaryAsync(string queueName, CancellationToken cancellationToken)
    {
        try
        {
            var count = await _deadLetterQueueGateway.GetMessageCountAsync(queueName, cancellationToken);
            return new DeadLetterQueueSummaryDto(queueName, count);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            return new DeadLetterQueueSummaryDto(queueName, 0, "Không thể tải tổng hợp Hàng đợi thư chết.");
        }
    }

    /// <summary>
    /// Lấy tổng hợp Kiểm tra sức khỏe.
    /// </summary>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Tổng hợp Kiểm tra sức khỏe.</returns>
    private async Task<HealthSummaryDto> GetHealthSummaryAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await _dashboardHealthSummaryReader.GetSummaryAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            return new HealthSummaryDto("Unhealthy", "Không thể tải trạng thái Kiểm tra sức khỏe.");
        }
    }

    /// <summary>
    /// Lấy số lượng theo trạng thái từ bản đồ tổng hợp.
    /// </summary>
    /// <param name="counts">Bản đồ số lượng theo trạng thái.</param>
    /// <param name="status">Tên trạng thái.</param>
    /// <returns>Số lượng tương ứng.</returns>
    private static int GetCount(IReadOnlyDictionary<string, int> counts, string status)
    {
        return counts.TryGetValue(status, out var count) ? count : 0;
    }
}
