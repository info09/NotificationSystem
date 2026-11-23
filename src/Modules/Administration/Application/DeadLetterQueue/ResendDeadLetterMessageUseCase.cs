using Administration.Application.Abstractions;
using Administration.Domain;
using BuildingBlocks.SharedKernel.Notifications;
using System.Text.Json;

namespace Administration.Application.DeadLetterQueue;

/// <summary>
/// Use case gửi lại Thông điệp từ Hàng đợi thư chết.
/// </summary>
public sealed class ResendDeadLetterMessageUseCase
{
    private static readonly HashSet<string> AllowedQueues = new(StringComparer.OrdinalIgnoreCase)
    {
        "notifications.email.dlq",
        "notifications.inapp.dlq"
    };

    private readonly IDeadLetterQueueGateway _gateway;
    private readonly IDeadLetterResendRequestRepository _resendRequestRepository;
    private readonly IAdministrationAuditLogWriter _auditLogWriter;
    private readonly INotificationResendStateService _notificationResendStateService;

    /// <summary>
    /// Khởi tạo use case gửi lại Thông điệp từ Hàng đợi thư chết.
    /// </summary>
    /// <param name="gateway">Cổng truy cập Hàng đợi thư chết.</param>
    /// <param name="resendRequestRepository">Repository lưu yêu cầu Gửi lại.</param>
    /// <param name="auditLogWriter">Bộ ghi Nhật ký kiểm toán.</param>
    /// <param name="notificationResendStateService">Dịch vụ đặt lại trạng thái Thông báo.</param>
    public ResendDeadLetterMessageUseCase(
        IDeadLetterQueueGateway gateway,
        IDeadLetterResendRequestRepository resendRequestRepository,
        IAdministrationAuditLogWriter auditLogWriter,
        INotificationResendStateService notificationResendStateService)
    {
        _gateway = gateway;
        _resendRequestRepository = resendRequestRepository;
        _auditLogWriter = auditLogWriter;
        _notificationResendStateService = notificationResendStateService;
    }

    /// <summary>
    /// Xử lý yêu cầu gửi lại Thông điệp từ Hàng đợi thư chết.
    /// </summary>
    /// <param name="queueName">Tên Hàng đợi thư chết.</param>
    /// <param name="messageId">Mã định danh Thông điệp.</param>
    /// <param name="request">Dữ liệu yêu cầu gửi lại.</param>
    /// <param name="caller">Ngữ cảnh người gọi.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả gửi lại Thông điệp.</returns>
    public async Task<AdministrationAppResult<ResendDeadLetterMessageResponse>> HandleAsync(
        string? queueName,
        string? messageId,
        ResendDeadLetterMessageRequest? request,
        AdministrationCallerContext caller,
        CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        if (string.IsNullOrWhiteSpace(queueName) || !AllowedQueues.Contains(queueName))
        {
            errors["queueName"] = ["Queue name is not supported."];
        }

        if (string.IsNullOrWhiteSpace(messageId))
        {
            errors["messageId"] = ["Message id is required."];
        }

        if (string.IsNullOrWhiteSpace(request?.IdempotencyKey))
        {
            errors["idempotencyKey"] = ["Idempotency key is required."];
        }

        if (string.IsNullOrWhiteSpace(caller.UserId))
        {
            errors["actor"] = ["Actor is required."];
        }

        if (errors.Count > 0)
        {
            return AdministrationAppResult<ResendDeadLetterMessageResponse>.Validation(errors);
        }

        var normalizedQueue = queueName!.Trim();
        var normalizedMessageId = messageId!.Trim();
        var normalizedIdempotencyKey = request!.IdempotencyKey!.Trim();
        var actorUserId = caller.UserId!.Trim();
        var actorRole = caller.Roles.FirstOrDefault() ?? string.Empty;

        var existing = await _resendRequestRepository.GetByIdempotencyKeyAsync(normalizedIdempotencyKey, cancellationToken);
        if (existing is not null)
        {
            var existingResponse = new ResendDeadLetterMessageResponse(
                existing.QueueName,
                existing.MessageId,
                existing.ResultMessageId ?? Guid.Empty,
                null,
                "Duplicate");
            await WriteAuditAsync(actorUserId, actorRole, normalizedQueue, normalizedMessageId, "Duplicate", "Idempotent replay.", caller.CorrelationId, existing.ResultMessageId, cancellationToken);
            return AdministrationAppResult<ResendDeadLetterMessageResponse>.Ok(existingResponse);
        }

        var resendResult = await _gateway.ResendAsync(normalizedQueue, normalizedMessageId, cancellationToken);
        if (resendResult is null)
        {
            await WriteAuditAsync(actorUserId, actorRole, normalizedQueue, normalizedMessageId, "NotFound", "Dead letter message not found.", caller.CorrelationId, null, cancellationToken);
            return AdministrationAppResult<ResendDeadLetterMessageResponse>.NotFound();
        }

        if (resendResult.NotificationId.HasValue)
        {
            await _notificationResendStateService.ResetForResendAsync(resendResult.NotificationId.Value, cancellationToken);
        }

        var resendRecord = DeadLetterResendRequestRecord.Create(
            normalizedIdempotencyKey,
            actorUserId,
            normalizedQueue,
            normalizedMessageId);
        resendRecord.MarkCompleted(resendResult.ResentMessageId);
        try
        {
            await _resendRequestRepository.AddAsync(resendRecord, cancellationToken);
        }
        catch (Exception)
        {
            var existingAfterConflict = await _resendRequestRepository.GetByIdempotencyKeyAsync(normalizedIdempotencyKey, cancellationToken);
            if (existingAfterConflict is not null)
            {
                await WriteAuditAsync(actorUserId, actorRole, normalizedQueue, normalizedMessageId, "Duplicate", "Idempotent replay after concurrent resend.", caller.CorrelationId, resendResult.ResentMessageId, cancellationToken);
                var duplicateResponse = new ResendDeadLetterMessageResponse(normalizedQueue, normalizedMessageId, resendResult.ResentMessageId, resendResult.NotificationId, "Duplicate");
                return AdministrationAppResult<ResendDeadLetterMessageResponse>.Ok(duplicateResponse);
            }

            throw;
        }

        await WriteAuditAsync(actorUserId, actorRole, normalizedQueue, normalizedMessageId, "Success", null, caller.CorrelationId, resendResult.ResentMessageId, cancellationToken);

        var response = new ResendDeadLetterMessageResponse(
            normalizedQueue,
            normalizedMessageId,
            resendResult.ResentMessageId,
            resendResult.NotificationId,
            "Resent");

        return AdministrationAppResult<ResendDeadLetterMessageResponse>.Ok(response);
    }

    /// <summary>
    /// Ghi Nhật ký kiểm toán cho thao tác gửi lại.
    /// </summary>
    /// <param name="actorUserId">Mã định danh người thực hiện.</param>
    /// <param name="actorRole">Vai trò người thực hiện.</param>
    /// <param name="queueName">Tên Hàng đợi thư chết.</param>
    /// <param name="messageId">Mã định danh Thông điệp.</param>
    /// <param name="result">Kết quả thao tác.</param>
    /// <param name="reason">Lý do hoặc lỗi.</param>
    /// <param name="correlationId">Mã tương quan.</param>
    /// <param name="resentMessageId">Mã định danh Thông điệp mới.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    private async Task WriteAuditAsync(
        string actorUserId,
        string actorRole,
        string queueName,
        string messageId,
        string result,
        string? reason,
        string? correlationId,
        Guid? resentMessageId,
        CancellationToken cancellationToken)
    {
        var metadata = JsonSerializer.Serialize(new
        {
            queueName,
            messageId,
            resentMessageId
        });

        var audit = AuditLogRecord.Create(
            actorUserId: actorUserId,
            actorRole: actorRole,
            action: "ResendDeadLetterMessage",
            targetType: "DeadLetterMessage",
            targetId: messageId,
            result: result,
            reason: reason,
            correlationId: correlationId,
            metadataJson: metadata);

        await _auditLogWriter.WriteAsync(audit, cancellationToken);
    }
}
