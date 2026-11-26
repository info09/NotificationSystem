using Administration.Application.Abstractions;

namespace Administration.Application.AuditLogs;

/// <summary>
/// Use case truy vấn Nhật ký kiểm toán.
/// </summary>
public sealed class GetAuditLogsUseCase
{
    private readonly IAuditLogRecordRepository _repository;

    /// <summary>
    /// Khởi tạo use case truy vấn Nhật ký kiểm toán.
    /// </summary>
    /// <param name="repository">Repository Nhật ký kiểm toán.</param>
    public GetAuditLogsUseCase(IAuditLogRecordRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Xử lý truy vấn danh sách Nhật ký kiểm toán.
    /// </summary>
    /// <param name="action">Hành động lọc.</param>
    /// <param name="targetType">Loại đối tượng lọc.</param>
    /// <param name="actorUserId">Người thực hiện lọc.</param>
    /// <param name="fromUtc">Thời điểm bắt đầu (UTC).</param>
    /// <param name="toUtc">Thời điểm kết thúc (UTC).</param>
    /// <param name="page">Số trang.</param>
    /// <param name="pageSize">Kích thước trang.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả truy vấn Nhật ký kiểm toán.</returns>
    public async Task<AdministrationAppResult<GetAuditLogsResponseDto>> HandleAsync(
        string? action,
        string? targetType,
        string? actorUserId,
        DateTime? fromUtc,
        DateTime? toUtc,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        if (page < 1)
        {
            errors["page"] = ["Page must be greater than or equal to 1."];
        }

        if (pageSize < 1 || pageSize > 100)
        {
            errors["pageSize"] = ["Page size must be between 1 and 100."];
        }

        if (fromUtc.HasValue && toUtc.HasValue && fromUtc.Value > toUtc.Value)
        {
            errors["fromUtc"] = ["fromUtc must be less than or equal to toUtc."];
        }

        if (errors.Count > 0)
        {
            return AdministrationAppResult<GetAuditLogsResponseDto>.Validation(errors);
        }

        var (items, totalCount) = await _repository.GetPagedAsync(
            action,
            targetType,
            actorUserId,
            fromUtc,
            toUtc,
            page,
            pageSize,
            cancellationToken);

        var response = new GetAuditLogsResponseDto(
            items.Select(x => new AuditLogResponseDto(
                x.Id,
                x.ActorUserId,
                x.ActorRole,
                x.Action,
                x.TargetType,
                x.TargetId,
                x.Result,
                x.Reason,
                x.CorrelationId,
                x.OccurredAtUtc)).ToList(),
            totalCount,
            page,
            pageSize);

        return AdministrationAppResult<GetAuditLogsResponseDto>.Ok(response);
    }
}
