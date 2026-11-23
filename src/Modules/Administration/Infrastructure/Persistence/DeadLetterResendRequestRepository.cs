using Administration.Application.Abstractions;
using Administration.Domain;
using Microsoft.EntityFrameworkCore;

namespace Administration.Infrastructure.Persistence;

/// <summary>
/// Repository lưu trữ yêu cầu Gửi lại từ Hàng đợi thư chết.
/// </summary>
public sealed class DeadLetterResendRequestRepository : IDeadLetterResendRequestRepository
{
    private readonly AdministrationDbContext _dbContext;

    /// <summary>
    /// Khởi tạo repository yêu cầu Gửi lại.
    /// </summary>
    /// <param name="dbContext">DbContext của module Administration.</param>
    public DeadLetterResendRequestRepository(AdministrationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Lấy bản ghi yêu cầu Gửi lại theo khóa idempotency.
    /// </summary>
    /// <param name="idempotencyKey">Khóa idempotency.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Bản ghi yêu cầu Gửi lại nếu tồn tại; ngược lại null.</returns>
    public async Task<DeadLetterResendRequestRecord?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken)
    {
        return await _dbContext.DeadLetterResendRequestRecords
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.IdempotencyKey == idempotencyKey, cancellationToken);
    }

    /// <summary>
    /// Thêm bản ghi yêu cầu Gửi lại.
    /// </summary>
    /// <param name="record">Bản ghi yêu cầu Gửi lại.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    public async Task AddAsync(DeadLetterResendRequestRecord record, CancellationToken cancellationToken)
    {
        _dbContext.DeadLetterResendRequestRecords.Add(record);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
