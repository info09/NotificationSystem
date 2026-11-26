namespace BuildingBlocks.SharedKernel.IntegrationEvents;

/// <summary>
/// Kết quả tạo Thông báo từ Sự kiện tích hợp.
/// </summary>
/// <param name="NotificationId">Mã định danh Thông báo.</param>
/// <param name="Status">Trạng thái Thông báo.</param>
/// <param name="Created">Thông báo mới được tạo hay đã tồn tại.</param>
public sealed record IntegrationNotificationCreationResult(
    Guid NotificationId,
    string Status,
    bool Created);
