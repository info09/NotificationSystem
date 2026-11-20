namespace Notification.Application.Notifications.GetNotificationById;

public sealed class GetNotificationByIdUseCase
{
    private readonly INotificationRepository _notificationRepository;

    public GetNotificationByIdUseCase(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

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
