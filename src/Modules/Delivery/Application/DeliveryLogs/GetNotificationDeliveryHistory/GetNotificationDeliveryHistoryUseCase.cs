using Delivery.Application.Abstractions;
using Microsoft.Extensions.Options;

namespace Delivery.Application.DeliveryLogs.GetNotificationDeliveryHistory;

/// <summary>
/// Use case lấy lịch sử gửi Thông báo.
/// </summary>
public sealed class GetNotificationDeliveryHistoryUseCase
{
    private const int MaxPageSize = 100;

    private readonly IDeliveryHistoryRepository _repository;
    private readonly DeliveryHistoryOptions _options;

    /// <summary>
    /// Khởi tạo use case lấy lịch sử gửi Thông báo.
    /// </summary>
    /// <param name="repository">Repository truy vấn lịch sử gửi.</param>
    /// <param name="options">Cấu hình lịch sử gửi.</param>
    public GetNotificationDeliveryHistoryUseCase(
        IDeliveryHistoryRepository repository,
        IOptions<DeliveryHistoryOptions> options)
    {
        _repository = repository;
        _options = options.Value;
    }

    /// <summary>
    /// Xử lý truy vấn lấy lịch sử gửi Thông báo.
    /// </summary>
    /// <param name="query">Truy vấn lịch sử gửi.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả lịch sử gửi Thông báo.</returns>
    public async Task<DeliveryHistoryAppResult<DeliveryHistoryResponseDto>> HandleAsync(
        GetNotificationDeliveryHistoryQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var errors = new Dictionary<string, string[]>();
        if (query.NotificationId == Guid.Empty)
        {
            errors["notificationId"] = ["NotificationId is required."];
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
            return DeliveryHistoryAppResult<DeliveryHistoryResponseDto>.Validation(errors);
        }

        var retentionDays = _options.RetentionDays > 0 ? _options.RetentionDays : 90;
        var fromUtc = DateTime.UtcNow.AddDays(-retentionDays);

        var logs = await _repository.GetLogsByNotificationIdAsync(query.NotificationId, fromUtc, cancellationToken);
        var (attempts, totalCount) = await _repository.GetAttemptsByNotificationIdAsync(
            query.NotificationId,
            fromUtc,
            query.Channel,
            query.Result,
            query.Page,
            query.PageSize,
            cancellationToken);

        var response = new DeliveryHistoryResponseDto(
            logs.Select(x => new DeliveryLogDto(
                x.Id,
                x.NotificationId,
                x.Channel,
                x.Result.ToString(),
                x.ErrorMessage,
                x.RetryCount,
                x.CorrelationId,
                x.CreatedAtUtc)).ToList(),
            new DeliveryAttemptsDto(
                attempts.Select(x => new DeliveryAttemptDto(
                    x.Id,
                    x.NotificationId,
                    x.Channel,
                    x.AttemptNumber,
                    x.Result.ToString(),
                    x.ErrorMessage,
                    x.CorrelationId,
                    x.AttemptedAtUtc)).ToList(),
                totalCount,
                query.Page,
                query.PageSize));

        return DeliveryHistoryAppResult<DeliveryHistoryResponseDto>.Ok(response);
    }
}
