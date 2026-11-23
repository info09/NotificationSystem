using Administration.Domain;

namespace Administration.Application.Abstractions;

/// <summary>
/// Repository truy cập bản ghi yêu cầu Gửi lại từ Hàng đợi thư chết.
/// </summary>
public interface IDeadLetterResendRequestRepository
{
    /// <summary>
    /// Lấy bản ghi yêu cầu Gửi lại theo khóa idempotency.
    /// </summary>
    /// <param name="idempotencyKey">Khóa idempotency.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Bản ghi yêu cầu Gửi lại nếu tồn tại; ngược lại null.</returns>
    Task<DeadLetterResendRequestRecord?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken);

    /// <summary>
    /// Thêm bản ghi yêu cầu Gửi lại.
    /// </summary>
    /// <param name="record">Bản ghi yêu cầu Gửi lại.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    Task AddAsync(DeadLetterResendRequestRecord record, CancellationToken cancellationToken);
}
