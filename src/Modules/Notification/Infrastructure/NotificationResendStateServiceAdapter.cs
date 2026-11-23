using BuildingBlocks.SharedKernel.Notifications;
using Notification.Infrastructure.Persistence;

namespace Notification.Infrastructure;

/// <summary>
/// Bộ điều hợp đặt lại trạng thái Thông báo để hỗ trợ Gửi lại.
/// </summary>
public sealed class NotificationResendStateServiceAdapter : INotificationResendStateService
{
    private readonly NotificationDbContext _dbContext;

    /// <summary>
    /// Khởi tạo bộ điều hợp đặt lại trạng thái Thông báo.
    /// </summary>
    /// <param name="dbContext">DbContext của module Notification.</param>
    public NotificationResendStateServiceAdapter(NotificationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Đặt lại trạng thái Thông báo về Chờ xử lý để gửi lại.
    /// </summary>
    /// <param name="notificationId">Mã định danh Thông báo.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns><c>true</c> nếu đặt lại thành công; ngược lại <c>false</c>.</returns>
    public async Task<bool> ResetForResendAsync(Guid notificationId, CancellationToken cancellationToken)
    {
        var notification = await _dbContext.Notifications.FindAsync([notificationId], cancellationToken);
        if (notification is null)
        {
            return false;
        }

        notification.MarkPendingForResend();
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
