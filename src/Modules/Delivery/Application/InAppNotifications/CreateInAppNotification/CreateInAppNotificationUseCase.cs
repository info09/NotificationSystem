using Delivery.Application.Abstractions;
using Delivery.Domain.InAppNotifications;

namespace Delivery.Application.InAppNotifications.CreateInAppNotification;

/// <summary>
/// Use case tạo Thông báo trong ứng dụng với kiểm tra idempotency.
/// </summary>
public sealed class CreateInAppNotificationUseCase
{
    private readonly IInAppNotificationRepository _repository;

    /// <summary>
    /// Khởi tạo use case tạo Thông báo trong ứng dụng.
    /// </summary>
    /// <param name="repository">Repository Thông báo trong ứng dụng.</param>
    public CreateInAppNotificationUseCase(IInAppNotificationRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Xử lý lệnh tạo Thông báo trong ứng dụng. Nếu đã tồn tại theo NotificationId và RecipientUserId thì trả về bản ghi hiện có.
    /// </summary>
    /// <param name="command">Lệnh tạo Thông báo trong ứng dụng.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả Thông báo trong ứng dụng.</returns>
    public async Task<InAppNotificationAppResult<InAppNotificationResponseDto>> HandleAsync(
        CreateInAppNotificationCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var errors = new Dictionary<string, string[]>();
        if (command.NotificationId == Guid.Empty)
        {
            errors["notificationId"] = ["NotificationId is required."];
        }

        if (string.IsNullOrWhiteSpace(command.RecipientUserId))
        {
            errors["recipientUserId"] = ["Recipient user id is required."];
        }

        if (errors.Count > 0)
        {
            return InAppNotificationAppResult<InAppNotificationResponseDto>.Validation(errors);
        }

        var recipientUserId = command.RecipientUserId!.Trim();
        var existing = await _repository.FindByNotificationAndRecipientAsync(command.NotificationId, recipientUserId, cancellationToken);
        if (existing is not null)
        {
            return InAppNotificationAppResult<InAppNotificationResponseDto>.Ok(InAppNotificationMapper.ToResponse(existing));
        }

        var entity = InAppNotification.Create(
            Guid.NewGuid(),
            command.NotificationId,
            recipientUserId,
            command.Title,
            command.Content);

        await _repository.AddAsync(entity, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return InAppNotificationAppResult<InAppNotificationResponseDto>.Created(InAppNotificationMapper.ToResponse(entity));
    }
}
