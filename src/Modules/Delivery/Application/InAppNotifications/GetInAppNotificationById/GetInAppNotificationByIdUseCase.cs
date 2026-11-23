using Delivery.Application.Abstractions;

namespace Delivery.Application.InAppNotifications.GetInAppNotificationById;

/// <summary>
/// Use case lấy chi tiết Thông báo trong ứng dụng.
/// </summary>
public sealed class GetInAppNotificationByIdUseCase
{
    private readonly IInAppNotificationRepository _repository;

    /// <summary>
    /// Khởi tạo use case lấy chi tiết Thông báo trong ứng dụng.
    /// </summary>
    /// <param name="repository">Repository Thông báo trong ứng dụng.</param>
    public GetInAppNotificationByIdUseCase(IInAppNotificationRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Xử lý truy vấn lấy chi tiết Thông báo trong ứng dụng.
    /// </summary>
    /// <param name="query">Truy vấn lấy chi tiết.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả chi tiết Thông báo trong ứng dụng.</returns>
    public async Task<InAppNotificationAppResult<InAppNotificationResponseDto>> HandleAsync(
        GetInAppNotificationByIdQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (string.IsNullOrWhiteSpace(query.RecipientUserId))
        {
            return InAppNotificationAppResult<InAppNotificationResponseDto>.Validation(
                new Dictionary<string, string[]> { ["recipientUserId"] = ["Recipient user id is required."] });
        }

        var entity = await _repository.GetByIdForRecipientAsync(query.Id, query.RecipientUserId.Trim(), cancellationToken);
        if (entity is null)
        {
            return InAppNotificationAppResult<InAppNotificationResponseDto>.NotFound();
        }

        return InAppNotificationAppResult<InAppNotificationResponseDto>.Ok(InAppNotificationMapper.ToResponse(entity));
    }
}
