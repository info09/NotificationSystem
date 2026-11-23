using Delivery.Application.Abstractions;

namespace Delivery.Application.InAppNotifications.GetInAppNotifications;

/// <summary>
/// Use case lấy danh sách Thông báo trong ứng dụng của Người dùng cuối.
/// </summary>
public sealed class GetInAppNotificationsUseCase
{
    private const int MaxPageSize = 100;

    private readonly IInAppNotificationRepository _repository;

    /// <summary>
    /// Khởi tạo use case lấy danh sách Thông báo trong ứng dụng.
    /// </summary>
    /// <param name="repository">Repository Thông báo trong ứng dụng.</param>
    public GetInAppNotificationsUseCase(IInAppNotificationRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Xử lý truy vấn lấy danh sách Thông báo trong ứng dụng.
    /// </summary>
    /// <param name="query">Truy vấn lấy danh sách.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả danh sách Thông báo trong ứng dụng.</returns>
    public async Task<InAppNotificationAppResult<GetInAppNotificationsResponseDto>> HandleAsync(
        GetInAppNotificationsQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(query.RecipientUserId))
        {
            errors["recipientUserId"] = ["Recipient user id is required."];
        }

        if (query.Page < 1)
        {
            errors["page"] = ["page must be greater than or equal to 1."];
        }

        if (query.PageSize < 1 || query.PageSize > MaxPageSize)
        {
            errors["pageSize"] = [$"pageSize must be between 1 and {MaxPageSize}."];
        }

        if (errors.Count > 0)
        {
            return InAppNotificationAppResult<GetInAppNotificationsResponseDto>.Validation(errors);
        }

        var (items, totalCount) = await _repository.GetPagedAsync(
            query.RecipientUserId!.Trim(),
            query.UnreadOnly,
            query.Page,
            query.PageSize,
            cancellationToken);

        var response = new GetInAppNotificationsResponseDto(
            items.Select(InAppNotificationMapper.ToResponse).ToList(),
            totalCount,
            query.Page,
            query.PageSize);

        return InAppNotificationAppResult<GetInAppNotificationsResponseDto>.Ok(response);
    }
}
