namespace BuildingBlocks.SharedKernel.Notifications;

/// <summary>
/// Cổng phân giải Mẫu thông báo.
/// </summary>
public interface INotificationTemplateResolver
{
    /// <summary>
    /// Lấy nội dung Mẫu thông báo theo mã định danh.
    /// </summary>
    /// <param name="templateId">Mã định danh Mẫu thông báo.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Nội dung mẫu hoặc <c>null</c> nếu không tồn tại.</returns>
    Task<NotificationTemplateContent?> ResolveAsync(Guid templateId, CancellationToken cancellationToken);
}
