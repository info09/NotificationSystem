namespace Notification.Application.Notifications.GetNotificationById;

/// <summary>
/// Use case xử lý lấy chi tiết Thông báo theo mã định danh.
/// </summary>
public sealed class GetNotificationByIdUseCase
{
    private readonly INotificationRepository _notificationRepository;

    /// <summary>
    /// Khởi tạo use case với repository Thông báo.
    /// </summary>
    /// <param name="notificationRepository">Repository Thông báo.</param>
    public GetNotificationByIdUseCase(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    /// <summary>
    /// Xử lý yêu cầu lấy chi tiết Thông báo.
    /// </summary>
    /// <param name="request">Yêu cầu lấy chi tiết Thông báo.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả chi tiết Thông báo.</returns>
    public async Task<NotificationAppResult<NotificationItemResponseDto>> HandleAsync(
        GetNotificationByIdAppRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var scopeRestriction = NotificationAccess.GetScopeRestriction(request.Caller);
        var notification = await _notificationRepository.GetByIdForScopeAsync(
            request.Id,
            scopeRestriction,
            cancellationToken);

        if (notification is null)
        {
            return NotificationAppResult<NotificationItemResponseDto>.NotFound();
        }

        return NotificationAppResult<NotificationItemResponseDto>.Ok(
            NotificationMapper.MapToItemResponse(notification));
    }
}
