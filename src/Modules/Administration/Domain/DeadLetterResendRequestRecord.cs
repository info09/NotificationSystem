using BuildingBlocks.SharedKernel.Entity;

namespace Administration.Domain;

/// <summary>
/// Bản ghi yêu cầu Gửi lại từ Hàng đợi thư chết để bảo đảm idempotency.
/// </summary>
public sealed class DeadLetterResendRequestRecord : Entity
{
    /// <summary>
    /// Khởi tạo bản ghi yêu cầu Gửi lại cho ORM.
    /// </summary>
    private DeadLetterResendRequestRecord()
    {
        IdempotencyKey = string.Empty;
        ActorUserId = string.Empty;
        QueueName = string.Empty;
        MessageId = string.Empty;
        Status = string.Empty;
    }

    /// <summary>
    /// Khởi tạo bản ghi yêu cầu Gửi lại.
    /// </summary>
    /// <param name="id">Mã định danh bản ghi.</param>
    /// <param name="idempotencyKey">Khóa idempotency.</param>
    /// <param name="actorUserId">Mã định danh người thực hiện.</param>
    /// <param name="queueName">Tên hàng đợi thư chết.</param>
    /// <param name="messageId">Mã định danh Thông điệp.</param>
    /// <param name="status">Trạng thái xử lý yêu cầu.</param>
    /// <param name="createdAtUtc">Thời điểm tạo (UTC).</param>
    /// <param name="completedAtUtc">Thời điểm hoàn tất (UTC).</param>
    /// <param name="resultMessageId">Mã định danh Thông điệp mới sau khi gửi lại.</param>
    private DeadLetterResendRequestRecord(
        Guid id,
        string idempotencyKey,
        string actorUserId,
        string queueName,
        string messageId,
        string status,
        DateTime createdAtUtc,
        DateTime? completedAtUtc,
        Guid? resultMessageId)
    {
        Id = id;
        IdempotencyKey = idempotencyKey;
        ActorUserId = actorUserId;
        QueueName = queueName;
        MessageId = messageId;
        Status = status;
        CreatedAtUtc = createdAtUtc;
        CompletedAtUtc = completedAtUtc;
        ResultMessageId = resultMessageId;
    }

    /// <summary>
    /// Khóa idempotency của yêu cầu Gửi lại.
    /// </summary>
    public string IdempotencyKey { get; private set; }

    /// <summary>
    /// Mã định danh người thực hiện yêu cầu.
    /// </summary>
    public string ActorUserId { get; private set; }

    /// <summary>
    /// Tên Hàng đợi thư chết.
    /// </summary>
    public string QueueName { get; private set; }

    /// <summary>
    /// Mã định danh Thông điệp trong Hàng đợi thư chết.
    /// </summary>
    public string MessageId { get; private set; }

    /// <summary>
    /// Trạng thái xử lý yêu cầu.
    /// </summary>
    public string Status { get; private set; }

    /// <summary>
    /// Thời điểm tạo yêu cầu (UTC).
    /// </summary>
    public DateTime CreatedAtUtc { get; private set; }

    /// <summary>
    /// Thời điểm hoàn tất yêu cầu (UTC).
    /// </summary>
    public DateTime? CompletedAtUtc { get; private set; }

    /// <summary>
    /// Mã định danh Thông điệp mới sau khi gửi lại.
    /// </summary>
    public Guid? ResultMessageId { get; private set; }

    /// <summary>
    /// Tạo bản ghi yêu cầu Gửi lại mới.
    /// </summary>
    /// <param name="idempotencyKey">Khóa idempotency.</param>
    /// <param name="actorUserId">Mã định danh người thực hiện.</param>
    /// <param name="queueName">Tên hàng đợi thư chết.</param>
    /// <param name="messageId">Mã định danh Thông điệp.</param>
    /// <param name="createdAtUtc">Thời điểm tạo (UTC).</param>
    /// <returns>Bản ghi yêu cầu Gửi lại mới.</returns>
    public static DeadLetterResendRequestRecord Create(
        string idempotencyKey,
        string actorUserId,
        string queueName,
        string messageId,
        DateTime? createdAtUtc = null)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            throw new ArgumentException("Idempotency key is required.", nameof(idempotencyKey));
        }

        if (string.IsNullOrWhiteSpace(actorUserId))
        {
            throw new ArgumentException("Actor user id is required.", nameof(actorUserId));
        }

        if (string.IsNullOrWhiteSpace(queueName))
        {
            throw new ArgumentException("Queue name is required.", nameof(queueName));
        }

        if (string.IsNullOrWhiteSpace(messageId))
        {
            throw new ArgumentException("Message id is required.", nameof(messageId));
        }

        return new DeadLetterResendRequestRecord(
            Guid.NewGuid(),
            idempotencyKey.Trim(),
            actorUserId.Trim(),
            queueName.Trim(),
            messageId.Trim(),
            "Completed",
            createdAtUtc ?? DateTime.UtcNow,
            createdAtUtc ?? DateTime.UtcNow,
            null);
    }

    /// <summary>
    /// Đánh dấu yêu cầu Gửi lại đã hoàn tất.
    /// </summary>
    /// <param name="resultMessageId">Mã định danh Thông điệp mới.</param>
    /// <param name="completedAtUtc">Thời điểm hoàn tất (UTC).</param>
    public void MarkCompleted(Guid resultMessageId, DateTime? completedAtUtc = null)
    {
        Status = "Completed";
        ResultMessageId = resultMessageId;
        CompletedAtUtc = completedAtUtc ?? DateTime.UtcNow;
    }
}
