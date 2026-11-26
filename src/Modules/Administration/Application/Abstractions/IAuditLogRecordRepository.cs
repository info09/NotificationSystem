using Administration.Domain;

namespace Administration.Application.Abstractions;

/// <summary>
/// Repository truy vấn Nhật ký kiểm toán.
/// </summary>
public interface IAuditLogRecordRepository
{
    /// <summary>
    /// Lấy danh sách Nhật ký kiểm toán có phân trang.
    /// </summary>
    /// <param name="action">Hành động lọc.</param>
    /// <param name="targetType">Loại đối tượng lọc.</param>
    /// <param name="actorUserId">Người thực hiện lọc.</param>
    /// <param name="fromUtc">Thời điểm bắt đầu (UTC).</param>
    /// <param name="toUtc">Thời điểm kết thúc (UTC).</param>
    /// <param name="page">Số trang.</param>
    /// <param name="pageSize">Kích thước trang.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Danh sách bản ghi và tổng số bản ghi.</returns>
    Task<(IReadOnlyList<AuditLogRecord> Items, int TotalCount)> GetPagedAsync(
        string? action,
        string? targetType,
        string? actorUserId,
        DateTime? fromUtc,
        DateTime? toUtc,
        int page,
        int pageSize,
        CancellationToken cancellationToken);
}
