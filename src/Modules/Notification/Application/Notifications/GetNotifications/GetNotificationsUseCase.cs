using System.Globalization;
using NotificationChannel = global::Notification.Domain.NotificationChannel;
using NotificationStatus = global::Notification.Domain.NotificationStatus;

namespace Notification.Application.Notifications.GetNotifications;

public sealed class GetNotificationsUseCase
{
    private const int DefaultPage = 1;
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 100;

    private static readonly string[] PrivilegedRoles = ["SystemAdministrator", "BusinessOperator"];

    private readonly INotificationRepository _notificationRepository;

    public GetNotificationsUseCase(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

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
            if (!TryParseChannel(request.Channel, out var channelValue))
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
            if (!TryParseUtcDateTime(request.CreatedFrom, out var fromValue))
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
            if (!TryParseUtcDateTime(request.CreatedTo, out var toValue))
            {
                errors["createdTo"] = ["createdTo must be a valid date/time."];
            }
            else
            {
                parsedCreatedToUtc = toValue;
            }
        }

        var page = request.Page == 0 ? DefaultPage : request.Page;
        var pageSize = request.PageSize == 0 ? DefaultPageSize : request.PageSize;

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

        var scopeRestriction = GetScopeRestriction(request.Caller);

        try
        {
            var query = new GetNotificationsQuery(
                parsedStatus,
                parsedChannel,
                parsedCreatedFromUtc,
                parsedCreatedToUtc,
                page,
                pageSize,
                scopeRestriction);

            var result = await GetPagedAsync(query, cancellationToken);
            var response = new GetNotificationsResponseDto(
                result.Items.Select(MapToResponseItem).ToList(),
                result.TotalCount,
                result.Page,
                result.PageSize);

            return NotificationAppResult<GetNotificationsResponseDto>.Ok(response);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return NotificationAppResult<GetNotificationsResponseDto>.Validation(
                new Dictionary<string, string[]>
                {
                    ["query"] = [ex.Message]
                });
        }
    }

    private async Task<PagedResult<NotificationDto>> GetPagedAsync(
        GetNotificationsQuery query,
        CancellationToken cancellationToken)
    {
        if (query.Page < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(query), "Page must be greater than or equal to 1.");
        }

        if (query.PageSize < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(query), "PageSize must be greater than or equal to 1.");
        }

        var (items, totalCount) = await _notificationRepository.GetPagedAsync(
            query.Status,
            query.Channel,
            query.CreatedFromUtc,
            query.CreatedToUtc,
            query.Page,
            query.PageSize,
            query.ScopeRestriction,
            cancellationToken);

        return new PagedResult<NotificationDto>(
            items.Select(NotificationMapper.MapToDto).ToList(),
            totalCount,
            query.Page,
            query.PageSize);
    }

    private static string? GetScopeRestriction(NotificationCallerContext? caller)
    {
        if (caller is not null && caller.Roles.Any(role => PrivilegedRoles.Contains(role, StringComparer.Ordinal)))
        {
            return null;
        }

        return caller?.Subject;
    }

    private static bool TryParseChannel(string channel, out NotificationChannel parsedChannel)
    {
        if (string.Equals(channel, "email", StringComparison.OrdinalIgnoreCase))
        {
            parsedChannel = NotificationChannel.Email;
            return true;
        }

        if (string.Equals(channel, "inapp", StringComparison.OrdinalIgnoreCase))
        {
            parsedChannel = NotificationChannel.InApp;
            return true;
        }

        parsedChannel = default;
        return false;
    }

    private static bool TryParseUtcDateTime(string value, out DateTime utcDateTime)
    {
        if (DateTimeOffset.TryParse(
            value,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
            out var dateTimeOffset))
        {
            utcDateTime = dateTimeOffset.UtcDateTime;
            return true;
        }

        utcDateTime = default;
        return false;
    }

    private static NotificationItemResponseDto MapToResponseItem(NotificationDto notification) => new(
        notification.Id,
        notification.Recipient,
        notification.Channel.ToString(),
        notification.Status.ToString(),
        notification.Content,
        notification.NotificationTemplateId,
        notification.CreatedAt);
}
