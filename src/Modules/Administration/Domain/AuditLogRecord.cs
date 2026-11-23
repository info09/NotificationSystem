using BuildingBlocks.SharedKernel.Entity;

namespace Administration.Domain;

/// <summary>
/// Bản ghi Nhật ký kiểm toán cho thao tác quản trị/vận hành nhạy cảm.
/// </summary>
public sealed class AuditLogRecord : Entity
{
    /// <summary>
    /// Khởi tạo bản ghi Nhật ký kiểm toán cho ORM.
    /// </summary>
    private AuditLogRecord()
    {
        ActorUserId = string.Empty;
        ActorRole = string.Empty;
        Action = string.Empty;
        TargetType = string.Empty;
        TargetId = string.Empty;
        Result = string.Empty;
    }

    /// <summary>
    /// Khởi tạo bản ghi Nhật ký kiểm toán.
    /// </summary>
    /// <param name="id">Mã định danh bản ghi.</param>
    /// <param name="actorUserId">Mã định danh người thực hiện.</param>
    /// <param name="actorRole">Vai trò người thực hiện.</param>
    /// <param name="action">Hành động thực hiện.</param>
    /// <param name="targetType">Loại đối tượng tác động.</param>
    /// <param name="targetId">Mã định danh đối tượng tác động.</param>
    /// <param name="result">Kết quả thực hiện.</param>
    /// <param name="reason">Lý do hoặc thông tin lỗi.</param>
    /// <param name="correlationId">Mã tương quan.</param>
    /// <param name="metadataJson">Metadata bổ sung dạng JSON.</param>
    /// <param name="occurredAtUtc">Thời điểm xảy ra (UTC).</param>
    private AuditLogRecord(
        Guid id,
        string actorUserId,
        string actorRole,
        string action,
        string targetType,
        string targetId,
        string result,
        string? reason,
        string? correlationId,
        string? metadataJson,
        DateTime occurredAtUtc)
    {
        Id = id;
        ActorUserId = actorUserId;
        ActorRole = actorRole;
        Action = action;
        TargetType = targetType;
        TargetId = targetId;
        Result = result;
        Reason = reason;
        CorrelationId = correlationId;
        MetadataJson = metadataJson;
        OccurredAtUtc = occurredAtUtc;
    }

    /// <summary>
    /// Mã định danh người thực hiện thao tác.
    /// </summary>
    public string ActorUserId { get; private set; }

    /// <summary>
    /// Vai trò của người thực hiện thao tác.
    /// </summary>
    public string ActorRole { get; private set; }

    /// <summary>
    /// Hành động được thực hiện.
    /// </summary>
    public string Action { get; private set; }

    /// <summary>
    /// Loại đối tượng bị tác động.
    /// </summary>
    public string TargetType { get; private set; }

    /// <summary>
    /// Mã định danh đối tượng bị tác động.
    /// </summary>
    public string TargetId { get; private set; }

    /// <summary>
    /// Kết quả của thao tác.
    /// </summary>
    public string Result { get; private set; }

    /// <summary>
    /// Lý do hoặc thông tin lỗi.
    /// </summary>
    public string? Reason { get; private set; }

    /// <summary>
    /// Mã tương quan phục vụ truy vết.
    /// </summary>
    public string? CorrelationId { get; private set; }

    /// <summary>
    /// Metadata bổ sung dạng JSON.
    /// </summary>
    public string? MetadataJson { get; private set; }

    /// <summary>
    /// Thời điểm xảy ra thao tác (UTC).
    /// </summary>
    public DateTime OccurredAtUtc { get; private set; }

    /// <summary>
    /// Tạo bản ghi Nhật ký kiểm toán mới.
    /// </summary>
    /// <param name="actorUserId">Mã định danh người thực hiện.</param>
    /// <param name="actorRole">Vai trò người thực hiện.</param>
    /// <param name="action">Hành động thực hiện.</param>
    /// <param name="targetType">Loại đối tượng tác động.</param>
    /// <param name="targetId">Mã định danh đối tượng tác động.</param>
    /// <param name="result">Kết quả thực hiện.</param>
    /// <param name="reason">Lý do hoặc thông tin lỗi.</param>
    /// <param name="correlationId">Mã tương quan.</param>
    /// <param name="metadataJson">Metadata bổ sung dạng JSON.</param>
    /// <param name="occurredAtUtc">Thời điểm xảy ra (UTC).</param>
    /// <returns>Bản ghi Nhật ký kiểm toán mới.</returns>
    public static AuditLogRecord Create(
        string actorUserId,
        string actorRole,
        string action,
        string targetType,
        string targetId,
        string result,
        string? reason = null,
        string? correlationId = null,
        string? metadataJson = null,
        DateTime? occurredAtUtc = null)
    {
        if (string.IsNullOrWhiteSpace(actorUserId))
        {
            throw new ArgumentException("Actor user id is required.", nameof(actorUserId));
        }

        if (string.IsNullOrWhiteSpace(action))
        {
            throw new ArgumentException("Action is required.", nameof(action));
        }

        if (string.IsNullOrWhiteSpace(targetType))
        {
            throw new ArgumentException("Target type is required.", nameof(targetType));
        }

        if (string.IsNullOrWhiteSpace(targetId))
        {
            throw new ArgumentException("Target id is required.", nameof(targetId));
        }

        if (string.IsNullOrWhiteSpace(result))
        {
            throw new ArgumentException("Result is required.", nameof(result));
        }

        return new AuditLogRecord(
            Guid.NewGuid(),
            actorUserId.Trim(),
            actorRole?.Trim() ?? string.Empty,
            action.Trim(),
            targetType.Trim(),
            targetId.Trim(),
            result.Trim(),
            string.IsNullOrWhiteSpace(reason) ? null : reason.Trim(),
            string.IsNullOrWhiteSpace(correlationId) ? null : correlationId.Trim(),
            string.IsNullOrWhiteSpace(metadataJson) ? null : metadataJson,
            occurredAtUtc ?? DateTime.UtcNow);
    }
}
