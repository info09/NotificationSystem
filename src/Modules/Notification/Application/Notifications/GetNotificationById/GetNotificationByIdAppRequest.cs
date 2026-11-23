namespace Notification.Application.Notifications.GetNotificationById;

/// <summary>
/// Yêu cầu lấy chi tiết Thông báo theo mã định danh.
/// </summary>
/// <param name="Id">Mã định danh Thông báo.</param>
/// <param name="Caller">Ngữ cảnh người gọi.</param>
public sealed record GetNotificationByIdAppRequest(Guid Id, NotificationCallerContext Caller);