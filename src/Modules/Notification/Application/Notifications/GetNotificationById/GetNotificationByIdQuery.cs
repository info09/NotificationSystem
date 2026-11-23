namespace Notification.Application.Notifications.GetNotificationById;

/// <summary>
/// Truy vấn lấy chi tiết Thông báo theo mã định danh và phạm vi.
/// </summary>
/// <param name="Id">Mã định danh Thông báo.</param>
/// <param name="ScopeRestriction">Giới hạn phạm vi truy cập.</param>
public sealed record GetNotificationByIdQuery(Guid Id, string? ScopeRestriction);