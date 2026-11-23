using NotificationChannel = global::Notification.Domain.NotificationChannel;
using NotificationStatus = global::Notification.Domain.NotificationStatus;

namespace Notification.Application.Notifications.GetNotifications;

/// <summary>
/// Use case xử lý lấy danh sách Thông báo.
/// </summary>
public sealed class GetNotificationsUseCase
{
    private const int MaxPageSize = 100;

    private readonly INotificationRepository _notificationRepository;

    /// <summary>
    /// Khởi tạo use case với repository Thông báo.
    /// </summary>
    /// <param name="notificationRepository">Repository Thông báo.</param>
    public GetNotificationsUseCase(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    /// <summary>
    /// Xử lý yêu cầu lấy danh sách Thông báo có lọc và phân trang.
    /// </summary>
    /// <param name="request">Yêu cầu lấy danh sách Thông báo.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả danh sách Thông báo.</returns>
    public async Task<NotificationAppResult<GetNotificationsResponseDto>> HandleAsync(
        GetNotificationsAppRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var errors = new Dictionary<string, string[]>();
        NotificationStatus? parsedStatus = null;
        NotificationChannel? parsedChannel = null;
        DateTime? parsedCreatedFromUtc = null;
        DateTime? parsedCreatedToUtc = null;

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            if (!Enum.TryParse<NotificationStatus>(request.Status, ignoreCase: true, out var statusValue))
            {
                errors["status"] = ["Status must be one of: Pending, Processing, Sent, Failed."];
            }
            else
            {
                parsedStatus = statusValue;
            }
        }

        if (!string.IsNullOrWhiteSpace(request.Channel))
        {
            if (!NotificationParsing.TryParseChannel(request.Channel, out var channelValue))
            {
                errors["channel"] = ["Channel must be 'email' or 'inapp'."];
            }
            else
            {
                parsedChannel = channelValue;
            }
        }

        if (!string.IsNullOrWhiteSpace(request.CreatedFrom))
        {
            if (!NotificationParsing.TryParseUtcDateTime(request.CreatedFrom, out var fromValue))
            {
                errors["createdFrom"] = ["createdFrom must be a valid date/time."];
            }
            else
            {
                parsedCreatedFromUtc = fromValue;
            }
        }

        if (!string.IsNullOrWhiteSpace(request.CreatedTo))
        {
            if (!NotificationParsing.TryParseUtcDateTime(request.CreatedTo, out var toValue))
            {
                errors["createdTo"] = ["createdTo must be a valid date/time."];
            }
            else
            {
                parsedCreatedToUtc = toValue;
            }
        }

        if (request.Page < 1)
        {
            errors["page"] = ["page must be greater than or equal to 1."];
        }

        if (request.PageSize < 1 || request.PageSize > MaxPageSize)
        {
            errors["pageSize"] = [$"pageSize must be between 1 and {MaxPageSize}."];
        }

        if (parsedCreatedFromUtc is not null && parsedCreatedToUtc is not null && parsedCreatedFromUtc > parsedCreatedToUtc)
        {
            errors["createdFrom"] = ["createdFrom must be less than or equal to createdTo."];
        }

        if (errors.Count > 0)
        {
            return NotificationAppResult<GetNotificationsResponseDto>.Validation(errors);
        }

        var scopeRestriction = NotificationAccess.GetScopeRestriction(request.Caller);

        var (items, totalCount) = await _notificationRepository.GetPagedAsync(
            parsedStatus,
            parsedChannel,
            parsedCreatedFromUtc,
            parsedCreatedToUtc,
            request.Page,
            request.PageSize,
            scopeRestriction,
            cancellationToken);

        var response = new GetNotificationsResponseDto(
            items.Select(NotificationMapper.MapToItemResponse).ToList(),
            totalCount,
            request.Page,
            request.PageSize);

        return NotificationAppResult<GetNotificationsResponseDto>.Ok(response);
    }
}
