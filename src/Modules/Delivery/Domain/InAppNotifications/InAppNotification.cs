using BuildingBlocks.SharedKernel.Entity;

namespace Delivery.Domain.InAppNotifications;

/// <summary>
/// Thực thể Thông báo trong ứng dụng dành cho Người dùng cuối.
/// </summary>
public sealed class InAppNotification : Entity
{
    /// <summary>
    /// Khởi tạo thực thể Thông báo trong ứng dụng cho ORM.
    /// </summary>
    private InAppNotification()
    {
        RecipientUserId = string.Empty;
    }

    /// <summary>
    /// Khởi tạo thực thể Thông báo trong ứng dụng với đầy đủ dữ liệu.
    /// </summary>
    /// <param name="id">Mã định danh Thông báo trong ứng dụng.</param>
    /// <param name="notificationId">Mã định danh Thông báo gốc.</param>
    /// <param name="recipientUserId">Mã định danh Người dùng cuối nhận Thông báo.</param>
    /// <param name="title">Tiêu đề Thông báo trong ứng dụng.</param>
    /// <param name="content">Nội dung Thông báo trong ứng dụng.</param>
    /// <param name="readStatus">Trạng thái đọc.</param>
    /// <param name="createdAtUtc">Thời điểm tạo (UTC).</param>
    /// <param name="readAtUtc">Thời điểm đọc (UTC).</param>
    private InAppNotification(
        Guid id,
        Guid notificationId,
        string recipientUserId,
        string? title,
        string? content,
        ReadStatus readStatus,
        DateTime createdAtUtc,
        DateTime? readAtUtc)
    {
        Id = id;
        NotificationId = notificationId;
        RecipientUserId = recipientUserId;
        Title = title;
        Content = content;
        ReadStatus = readStatus;
        CreatedAtUtc = createdAtUtc;
        ReadAtUtc = readAtUtc;
    }

    /// <summary>
    /// Mã định danh Thông báo gốc.
    /// </summary>
    public Guid NotificationId { get; private set; }

    /// <summary>
    /// Mã định danh Người dùng cuối nhận Thông báo.
    /// </summary>
    public string RecipientUserId { get; private set; }

    /// <summary>
    /// Tiêu đề Thông báo trong ứng dụng.
    /// </summary>
    public string? Title { get; private set; }

    /// <summary>
    /// Nội dung Thông báo trong ứng dụng.
    /// </summary>
    public string? Content { get; private set; }

    /// <summary>
    /// Trạng thái đọc của Thông báo trong ứng dụng.
    /// </summary>
    public ReadStatus ReadStatus { get; private set; }

    /// <summary>
    /// Thời điểm tạo Thông báo trong ứng dụng (UTC).
    /// </summary>
    public DateTime CreatedAtUtc { get; private set; }

    /// <summary>
    /// Thời điểm Người dùng cuối đọc Thông báo (UTC).
    /// </summary>
    public DateTime? ReadAtUtc { get; private set; }

    /// <summary>
    /// Tạo mới Thông báo trong ứng dụng ở trạng thái chưa đọc.
    /// </summary>
    /// <param name="id">Mã định danh Thông báo trong ứng dụng.</param>
    /// <param name="notificationId">Mã định danh Thông báo gốc.</param>
    /// <param name="recipientUserId">Mã định danh Người dùng cuối nhận Thông báo.</param>
    /// <param name="title">Tiêu đề Thông báo trong ứng dụng.</param>
    /// <param name="content">Nội dung Thông báo trong ứng dụng.</param>
    /// <param name="createdAtUtc">Thời điểm tạo, mặc định là UTC hiện tại.</param>
    /// <returns>Thực thể Thông báo trong ứng dụng vừa được tạo.</returns>
    public static InAppNotification Create(
        Guid id,
        Guid notificationId,
        string recipientUserId,
        string? title,
        string? content,
        DateTime? createdAtUtc = null)
    {
        if (notificationId == Guid.Empty)
        {
            throw new ArgumentException("NotificationId is required.", nameof(notificationId));
        }

        if (string.IsNullOrWhiteSpace(recipientUserId))
        {
            throw new ArgumentException("Recipient user id is required.", nameof(recipientUserId));
        }

        return new InAppNotification(
            id,
            notificationId,
            recipientUserId.Trim(),
            string.IsNullOrWhiteSpace(title) ? null : title.Trim(),
            content,
            ReadStatus.Unread,
            createdAtUtc ?? DateTime.UtcNow,
            null);
    }
}
