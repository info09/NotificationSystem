namespace Administration.Application.DeadLetterQueue;

/// <summary>
/// Thông tin tóm tắt một Thông điệp trong Hàng đợi thư chết.
/// </summary>
/// <param name="QueueName">Tên Hàng đợi thư chết.</param>
/// <param name="MessageId">Mã định danh Thông điệp.</param>
/// <param name="NotificationId">Mã định danh Thông báo.</param>
/// <param name="Channel">Kênh gửi.</param>
/// <param name="RoutingKey">Routing key.</param>
/// <param name="EventType">Loại sự kiện.</param>
/// <param name="Reason">Lý do vào Hàng đợi thư chết.</param>
/// <param name="DeadLetteredAtUtc">Thời điểm vào Hàng đợi thư chết (UTC).</param>
/// <param name="DeathCount">Số lần dead-letter.</param>
/// <param name="Payload">Nội dung payload.</param>
public sealed record DeadLetterQueueMessageDto(
    string QueueName,
    string MessageId,
    string? NotificationId,
    string? Channel,
    string? RoutingKey,
    string? EventType,
    string? Reason,
    DateTime? DeadLetteredAtUtc,
    long? DeathCount,
    string? Payload);

/// <summary>
/// Kết quả truy vấn danh sách Thông điệp trong Hàng đợi thư chết.
/// </summary>
/// <param name="QueueName">Tên Hàng đợi thư chết.</param>
/// <param name="Items">Danh sách Thông điệp.</param>
/// <param name="Count">Số lượng Thông điệp trả về.</param>
public sealed record GetDeadLetterQueueMessagesResponse(
    string QueueName,
    IReadOnlyList<DeadLetterQueueMessageDto> Items,
    int Count);

/// <summary>
/// Yêu cầu gửi lại Thông điệp từ Hàng đợi thư chết.
/// </summary>
/// <param name="IdempotencyKey">Khóa idempotency.</param>
public sealed record ResendDeadLetterMessageRequest(string? IdempotencyKey);

/// <summary>
/// Kết quả gửi lại Thông điệp từ Hàng đợi thư chết.
/// </summary>
/// <param name="QueueName">Tên Hàng đợi thư chết.</param>
/// <param name="MessageId">Mã định danh Thông điệp gốc.</param>
/// <param name="ResentMessageId">Mã định danh Thông điệp mới.</param>
/// <param name="NotificationId">Mã định danh Thông báo.</param>
/// <param name="Status">Trạng thái xử lý.</param>
public sealed record ResendDeadLetterMessageResponse(
    string QueueName,
    string MessageId,
    Guid ResentMessageId,
    Guid? NotificationId,
    string Status);
