using Administration.Domain;

namespace Administration.Application.Abstractions;

/// <summary>
/// Bộ ghi Nhật ký kiểm toán cho module Administration.
/// </summary>
public interface IAdministrationAuditLogWriter
{
    /// <summary>
    /// Ghi bản ghi Nhật ký kiểm toán.
    /// </summary>
    /// <param name="record">Bản ghi Nhật ký kiểm toán.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    Task WriteAsync(AuditLogRecord record, CancellationToken cancellationToken);
}
