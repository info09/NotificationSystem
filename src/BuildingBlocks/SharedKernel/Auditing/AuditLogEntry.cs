namespace BuildingBlocks.SharedKernel.Auditing;

/// <summary>
/// Thông tin ghi Nhật ký kiểm toán.
/// </summary>
/// <param name="ActorUserId">Mã định danh người thực hiện.</param>
/// <param name="ActorRole">Vai trò người thực hiện.</param>
/// <param name="Action">Hành động thực hiện.</param>
/// <param name="TargetType">Loại đối tượng tác động.</param>
/// <param name="TargetId">Mã định danh đối tượng tác động.</param>
/// <param name="Result">Kết quả thực hiện.</param>
/// <param name="Reason">Lý do hoặc thông tin lỗi.</param>
/// <param name="CorrelationId">Mã tương quan.</param>
/// <param name="MetadataJson">Metadata bổ sung dạng JSON.</param>
public sealed record AuditLogEntry(
    string ActorUserId,
    string? ActorRole,
    string Action,
    string TargetType,
    string TargetId,
    string Result,
    string? Reason = null,
    string? CorrelationId = null,
    string? MetadataJson = null);
