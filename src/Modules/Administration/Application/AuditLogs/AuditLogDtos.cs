namespace Administration.Application.AuditLogs;

/// <summary>
/// Dữ liệu phản hồi Nhật ký kiểm toán.
/// </summary>
/// <param name="Id">Mã định danh bản ghi.</param>
/// <param name="ActorUserId">Mã định danh người thực hiện.</param>
/// <param name="ActorRole">Vai trò người thực hiện.</param>
/// <param name="Action">Hành động thực hiện.</param>
/// <param name="TargetType">Loại đối tượng tác động.</param>
/// <param name="TargetId">Mã định danh đối tượng tác động.</param>
/// <param name="Result">Kết quả thực hiện.</param>
/// <param name="Reason">Lý do hoặc thông tin lỗi.</param>
/// <param name="CorrelationId">Mã tương quan.</param>
/// <param name="OccurredAtUtc">Thời điểm xảy ra (UTC).</param>
public sealed record AuditLogResponseDto(
    Guid Id,
    string ActorUserId,
    string ActorRole,
    string Action,
    string TargetType,
    string TargetId,
    string Result,
    string? Reason,
    string? CorrelationId,
    DateTime OccurredAtUtc);

/// <summary>
/// Dữ liệu phản hồi danh sách Nhật ký kiểm toán.
/// </summary>
/// <param name="Items">Danh sách bản ghi.</param>
/// <param name="TotalCount">Tổng số bản ghi.</param>
/// <param name="Page">Số trang.</param>
/// <param name="PageSize">Kích thước trang.</param>
public sealed record GetAuditLogsResponseDto(
    IReadOnlyList<AuditLogResponseDto> Items,
    int TotalCount,
    int Page,
    int PageSize);
