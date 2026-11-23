namespace Administration.Application.DeadLetterQueue;

/// <summary>
/// Cổng truy cập Hàng đợi thư chết để tra cứu và gửi lại Thông điệp.
/// </summary>
public interface IDeadLetterQueueGateway
{
    /// <summary>
    /// Lấy danh sách Thông điệp trong Hàng đợi thư chết.
    /// </summary>
    /// <param name="queueName">Tên Hàng đợi thư chết.</param>
    /// <param name="limit">Số lượng tối đa.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Danh sách Thông điệp.</returns>
    Task<IReadOnlyList<DeadLetterQueueMessageDto>> GetMessagesAsync(string queueName, int limit, CancellationToken cancellationToken);

    /// <summary>
    /// Gửi lại Thông điệp từ Hàng đợi thư chết.
    /// </summary>
    /// <param name="queueName">Tên Hàng đợi thư chết.</param>
    /// <param name="messageId">Mã định danh Thông điệp.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả gửi lại, hoặc null nếu không tìm thấy.</returns>
    Task<DeadLetterResendGatewayResult?> ResendAsync(string queueName, string messageId, CancellationToken cancellationToken);
}

/// <summary>
/// Kết quả gửi lại Thông điệp từ cổng Hàng đợi thư chết.
/// </summary>
/// <param name="MessageId">Mã định danh Thông điệp gốc.</param>
/// <param name="ResentMessageId">Mã định danh Thông điệp mới.</param>
/// <param name="NotificationId">Mã định danh Thông báo.</param>
/// <param name="QueueName">Tên Hàng đợi thư chết.</param>
public sealed record DeadLetterResendGatewayResult(
    string MessageId,
    Guid ResentMessageId,
    Guid? NotificationId,
    string QueueName);
