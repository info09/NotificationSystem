using Delivery.Application.Abstractions;
using Delivery.Domain.DeliveryLogs;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Infrastructure.Persistence;

/// <summary>
/// Ghi nhận Lần thử gửi và cập nhật Nhật ký gửi thông báo.
/// </summary>
public sealed class DeliveryLogRecorder : IDeliveryLogRecorder
{
    private readonly DeliveryDbContext _dbContext;

    /// <summary>
    /// Khởi tạo recorder với DbContext của module Delivery.
    /// </summary>
    /// <param name="dbContext">DbContext của module Delivery.</param>
    public DeliveryLogRecorder(DeliveryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Ghi nhận một Lần thử gửi và cập nhật Nhật ký gửi thông báo.
    /// </summary>
    /// <param name="request">Yêu cầu ghi nhận Lần thử gửi.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    public async Task RecordAttemptAsync(RecordDeliveryAttemptRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var attemptedAtUtc = request.AttemptedAtUtc ?? DateTime.UtcNow;
        var attempt = DeliveryAttempt.Create(
            Guid.NewGuid(),
            request.NotificationId,
            request.Channel,
            request.AttemptNumber,
            request.Result,
            request.ErrorMessage,
            request.CorrelationId,
            attemptedAtUtc);

        _dbContext.DeliveryAttempts.Add(attempt);

        var log = await _dbContext.NotificationDeliveryLogs
            .FirstOrDefaultAsync(
                x => x.NotificationId == request.NotificationId && x.Channel == request.Channel,
                cancellationToken);

        if (log is null)
        {
            log = NotificationDeliveryLog.Create(
                Guid.NewGuid(),
                request.NotificationId,
                request.Channel,
                request.Result,
                request.ErrorMessage,
                Math.Max(0, request.AttemptNumber - 1),
                request.CorrelationId,
                attemptedAtUtc);
            _dbContext.NotificationDeliveryLogs.Add(log);
        }
        else
        {
            log.Update(
                request.Result,
                request.ErrorMessage,
                Math.Max(log.RetryCount, request.AttemptNumber - 1),
                request.CorrelationId);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
