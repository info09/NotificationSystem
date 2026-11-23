using Delivery.Application.Abstractions;

namespace Delivery.Application.InAppNotifications.GetUnreadInAppNotificationCount;

/// <summary>
/// Use case lấy số lượng Thông báo trong ứng dụng chưa đọc.
/// </summary>
public sealed class GetUnreadInAppNotificationCountUseCase
{
    private readonly IInAppNotificationRepository _repository;

    /// <summary>
    /// Khởi tạo use case lấy số lượng Thông báo trong ứng dụng chưa đọc.
    /// </summary>
    /// <param name="repository">Repository Thông báo trong ứng dụng.</param>
    public GetUnreadInAppNotificationCountUseCase(IInAppNotificationRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Xử lý truy vấn lấy số lượng Thông báo trong ứng dụng chưa đọc.
    /// </summary>
    /// <param name="query">Truy vấn lấy số lượng chưa đọc.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả số lượng Thông báo chưa đọc.</returns>
    public async Task<InAppNotificationAppResult<UnreadInAppNotificationCountResponseDto>> HandleAsync(
        GetUnreadInAppNotificationCountQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (string.IsNullOrWhiteSpace(query.RecipientUserId))
        {
            return InAppNotificationAppResult<UnreadInAppNotificationCountResponseDto>.Validation(
                new Dictionary<string, string[]> { ["recipientUserId"] = ["Recipient user id is required."] });
        }

        var count = await _repository.GetUnreadCountAsync(query.RecipientUserId.Trim(), cancellationToken);
        return InAppNotificationAppResult<UnreadInAppNotificationCountResponseDto>.Ok(
            new UnreadInAppNotificationCountResponseDto(count));
    }
}
