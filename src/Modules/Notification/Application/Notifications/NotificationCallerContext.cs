namespace Notification.Application.Notifications;

/// <summary>
/// Ngữ cảnh người gọi dùng để kiểm tra phạm vi truy cập Thông báo.
/// </summary>
/// <param name="Subject">Định danh người gọi.</param>
/// <param name="Roles">Danh sách vai trò của người gọi.</param>
public sealed record NotificationCallerContext(string? Subject, IReadOnlyCollection<string> Roles)
{
    /// <summary>
    /// Ngữ cảnh rỗng khi không có thông tin người gọi.
    /// </summary>
    public static NotificationCallerContext Empty { get; } = new(null, Array.Empty<string>());
}