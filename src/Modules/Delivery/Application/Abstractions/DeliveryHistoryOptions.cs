namespace Delivery.Application.Abstractions;

/// <summary>
/// Cấu hình truy vấn lịch sử gửi Thông báo.
/// </summary>
public sealed class DeliveryHistoryOptions
{
    /// <summary>
    /// Khóa cấu hình trong file settings.
    /// </summary>
    public const string SectionName = "Delivery";

    /// <summary>
    /// Số ngày lưu giữ lịch sử gửi Thông báo.
    /// </summary>
    public int RetentionDays { get; set; } = 90;
}
