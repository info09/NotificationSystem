namespace BuildingBlocks.SharedKernel.Auditing;

/// <summary>
/// Cổng ghi Nhật ký kiểm toán.
/// </summary>
public interface IAuditLogRecorder
{
    /// <summary>
    /// Ghi bản ghi Nhật ký kiểm toán.
    /// </summary>
    /// <param name="entry">Thông tin ghi Nhật ký kiểm toán.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    Task RecordAsync(AuditLogEntry entry, CancellationToken cancellationToken);
}
