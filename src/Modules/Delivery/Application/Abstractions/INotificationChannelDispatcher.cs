namespace Delivery.Application.Abstractions;

/// <summary>
/// Abstraction dispatch Thông báo theo kênh.
/// </summary>
public interface INotificationChannelDispatcher
{
    /// <summary>
    /// Dispatch Thông báo theo kênh tương ứng.
    /// </summary>
    /// <param name="request">Yêu cầu dispatch Thông báo.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    Task DispatchAsync(NotificationChannelDispatchRequest request, CancellationToken cancellationToken);
}
