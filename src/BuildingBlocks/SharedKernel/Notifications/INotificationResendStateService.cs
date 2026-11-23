namespace BuildingBlocks.SharedKernel.Notifications;

/// <summary>
/// Dịch vụ đặt lại trạng thái Thông báo để hỗ trợ Gửi lại.
/// </summary>
public interface INotificationResendStateService
{
    /// <summary>
    /// Đặt lại trạng thái Thông báo về Chờ xử lý để gửi lại.
    /// </summary>
    /// <param name="notificationId">Mã định danh Thông báo.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns><c>true</c> nếu đặt lại thành công; ngược lại <c>false</c>.</returns>
    Task<bool> ResetForResendAsync(Guid notificationId, CancellationToken cancellationToken);
}
