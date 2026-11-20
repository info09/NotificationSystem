namespace Notification.Application.Notifications.GetNotificationById;

public sealed class GetNotificationByIdUseCase
{
    private static readonly string[] PrivilegedRoles = ["SystemAdministrator", "BusinessOperator"];

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

        var scopeRestriction = GetScopeRestriction(request.Caller);
        var query = new GetNotificationByIdQuery(request.Id, scopeRestriction);

        var notification = await _notificationRepository.GetByIdForScopeAsync(
            query.Id,
            query.ScopeRestriction,
            cancellationToken);

        if (notification is null)
        {
            return NotificationAppResult<NotificationItemResponseDto>.NotFound();
        }

        var dto = NotificationMapper.MapToDto(notification);
        var response = new NotificationItemResponseDto(
            dto.Id,
            dto.Recipient,
            dto.Channel.ToString(),
            dto.Status.ToString(),
            dto.Content,
            dto.NotificationTemplateId,
            dto.CreatedAt);

        return NotificationAppResult<NotificationItemResponseDto>.Ok(response);
    }

    private static string? GetScopeRestriction(NotificationCallerContext? caller)
    {
        if (caller is not null && caller.Roles.Any(role => PrivilegedRoles.Contains(role, StringComparer.Ordinal)))
        {
            return null;
        }

        return caller?.Subject;
    }
}
