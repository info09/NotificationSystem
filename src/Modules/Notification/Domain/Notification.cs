using BuildingBlocks.SharedKernel.Entity;

namespace Notification.Domain;

/// <summary>
/// Thực thể domain đại diện cho Thông báo.
/// </summary>
public sealed class Notification : Entity
{
    /// <summary>
    /// Khởi tạo thực thể Thông báo cho ORM.
    /// </summary>
    private Notification()
    {
        Recipient = null!;
        IdempotencyScope = string.Empty;
        IdempotencyKey = string.Empty;
    }

    /// <summary>
    /// Khởi tạo thực thể Thông báo với đầy đủ dữ liệu.
    /// </summary>
    /// <param name="id">Mã định danh Thông báo.</param>
    /// <param name="recipient">Người nhận Thông báo.</param>
    /// <param name="channel">Kênh gửi Thông báo.</param>
    /// <param name="status">Trạng thái Thông báo.</param>
    /// <param name="content">Nội dung Thông báo.</param>
    /// <param name="notificationTemplateId">Mã Mẫu thông báo liên kết.</param>
    /// <param name="idempotencyScope">Phạm vi idempotency.</param>
    /// <param name="idempotencyKey">Khóa idempotency.</param>
    /// <param name="createdAt">Thời điểm tạo Thông báo.</param>
    private Notification(
        Guid id,
        Recipient recipient,
        NotificationChannel channel,
        NotificationStatus status,
        string? content,
        Guid? notificationTemplateId,
        string idempotencyScope,
        string idempotencyKey,
        DateTime createdAt)
    {
        Id = id;
        Recipient = recipient;
        Channel = channel;
        Status = status;
        Content = content;
        NotificationTemplateId = notificationTemplateId;
        IdempotencyScope = idempotencyScope;
        IdempotencyKey = idempotencyKey;
        CreatedAt = createdAt;
    }

    /// <summary>
    /// Người nhận Thông báo.
    /// </summary>
    public Recipient Recipient { get; private set; }

    /// <summary>
    /// Kênh gửi Thông báo.
    /// </summary>
    public NotificationChannel Channel { get; private set; }

    /// <summary>
    /// Trạng thái hiện tại của Thông báo.
    /// </summary>
    public NotificationStatus Status { get; private set; }

    /// <summary>
    /// Nội dung Thông báo.
    /// </summary>
    public string? Content { get; private set; }

    /// <summary>
    /// Mã Mẫu thông báo liên kết.
    /// </summary>
    public Guid? NotificationTemplateId { get; private set; }

    /// <summary>
    /// Phạm vi idempotency của yêu cầu tạo Thông báo.
    /// </summary>
    public string IdempotencyScope { get; private set; }

    /// <summary>
    /// Khóa idempotency của yêu cầu tạo Thông báo.
    /// </summary>
    public string IdempotencyKey { get; private set; }

    /// <summary>
    /// Thời điểm tạo Thông báo (UTC).
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Tạo mới thực thể Thông báo ở trạng thái Chờ xử lý.
    /// </summary>
    /// <param name="id">Mã định danh Thông báo.</param>
    /// <param name="recipient">Người nhận Thông báo.</param>
    /// <param name="channel">Kênh gửi Thông báo.</param>
    /// <param name="content">Nội dung Thông báo.</param>
    /// <param name="notificationTemplateId">Mã Mẫu thông báo liên kết.</param>
    /// <param name="idempotencyScope">Phạm vi idempotency.</param>
    /// <param name="idempotencyKey">Khóa idempotency.</param>
    /// <param name="createdAt">Thời điểm tạo, mặc định là UTC hiện tại.</param>
    /// <returns>Thực thể Thông báo vừa được tạo.</returns>
    public static Notification Create(
        Guid id,
        Recipient recipient,
        NotificationChannel channel,
        string? content,
        Guid? notificationTemplateId,
        string idempotencyScope,
        string idempotencyKey,
        DateTime? createdAt = null)
    {
        ArgumentNullException.ThrowIfNull(recipient);

        if (string.IsNullOrWhiteSpace(content) && notificationTemplateId is null)
        {
            throw new ArgumentException("Notification content or notification template reference is required.");
        }

        if (string.IsNullOrWhiteSpace(idempotencyScope))
        {
            throw new ArgumentException("Idempotency scope is required.", nameof(idempotencyScope));
        }

        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            throw new ArgumentException("Idempotency key is required.", nameof(idempotencyKey));
        }

        return new Notification(
            id,
            recipient,
            channel,
            NotificationStatus.Pending,
            content,
            notificationTemplateId,
            idempotencyScope,
            idempotencyKey,
            createdAt ?? DateTime.UtcNow);
    }

    /// <summary>
    /// Chuyển Thông báo sang trạng thái Đang xử lý.
    /// </summary>
    public void MarkProcessing()
    {
        if (Status != NotificationStatus.Pending)
        {
            throw new InvalidOperationException("Only pending notifications can be marked as processing.");
        }

        Status = NotificationStatus.Processing;
    }

    /// <summary>
    /// Chuyển Thông báo sang trạng thái Đã gửi.
    /// </summary>
    public void MarkSent()
    {
        if (Status != NotificationStatus.Processing)
        {
            throw new InvalidOperationException("Only processing notifications can be marked as sent.");
        }

        Status = NotificationStatus.Sent;
    }

    /// <summary>
    /// Chuyển Thông báo sang trạng thái Thất bại.
    /// </summary>
    public void MarkFailed()
    {
        if (Status != NotificationStatus.Processing)
        {
            throw new InvalidOperationException("Only processing notifications can be marked as failed.");
        }

        Status = NotificationStatus.Failed;
    }

    /// <summary>
    /// Chuyển Thông báo về trạng thái Chờ xử lý để gửi lại.
    /// </summary>
    public void MarkPendingForResend()
    {
        if (Status is not (NotificationStatus.Failed or NotificationStatus.Sent or NotificationStatus.Processing))
        {
            throw new InvalidOperationException("Only failed, sent, or processing notifications can be marked as pending for resend.");
        }

        Status = NotificationStatus.Pending;
    }
}
