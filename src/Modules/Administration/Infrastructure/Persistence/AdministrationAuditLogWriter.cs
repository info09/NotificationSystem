using Administration.Application.Abstractions;
using Administration.Domain;
using Microsoft.EntityFrameworkCore;

namespace Administration.Infrastructure.Persistence;

/// <summary>
/// Bộ ghi Nhật ký kiểm toán sử dụng DbContext của module Administration.
/// </summary>
public sealed class AdministrationAuditLogWriter : IAdministrationAuditLogWriter
{
    private readonly AdministrationDbContext _dbContext;

    /// <summary>
    /// Khởi tạo bộ ghi Nhật ký kiểm toán.
    /// </summary>
    /// <param name="dbContext">DbContext của module Administration.</param>
    public AdministrationAuditLogWriter(AdministrationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Ghi bản ghi Nhật ký kiểm toán.
    /// </summary>
    /// <param name="record">Bản ghi Nhật ký kiểm toán.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    public async Task WriteAsync(AuditLogRecord record, CancellationToken cancellationToken)
    {
        _dbContext.AuditLogRecords.Add(record);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
