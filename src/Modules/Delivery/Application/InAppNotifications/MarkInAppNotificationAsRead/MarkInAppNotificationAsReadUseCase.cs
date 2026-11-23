using Delivery.Application.Abstractions;

namespace Delivery.Application.InAppNotifications.MarkInAppNotificationAsRead;

/// <summary>
/// Use case đánh dấu Thông báo trong ứng dụng là đã đọc.
/// </summary>
public sealed class MarkInAppNotificationAsReadUseCase
{
    private readonly IInAppNotificationRepository _repository;

    /// <summary>
    /// Khởi tạo use case đánh dấu đã đọc.
    /// </summary>
    /// <param name="repository">Repository Thông báo trong ứng dụng.</param>
    public MarkInAppNotificationAsReadUseCase(IInAppNotificationRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Xử lý lệnh đánh dấu Thông báo trong ứng dụng là đã đọc.
    /// </summary>
    /// <param name="command">Lệnh đánh dấu đã đọc.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả Thông báo trong ứng dụng sau khi cập nhật.</returns>
    public async Task<InAppNotificationAppResult<InAppNotificationResponseDto>> HandleAsync(
        MarkInAppNotificationAsReadCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.RecipientUserId))
        {
            return InAppNotificationAppResult<InAppNotificationResponseDto>.Validation(
                new Dictionary<string, string[]> { ["recipientUserId"] = ["Recipient user id is required."] });
        }

        var entity = await _repository.GetTrackedByIdForRecipientAsync(command.Id, command.RecipientUserId.Trim(), cancellationToken);
        if (entity is null)
        {
            return InAppNotificationAppResult<InAppNotificationResponseDto>.NotFound();
        }

        entity.MarkAsRead();
        await _repository.SaveChangesAsync(cancellationToken);

        return InAppNotificationAppResult<InAppNotificationResponseDto>.Ok(InAppNotificationMapper.ToResponse(entity));
    }
}
