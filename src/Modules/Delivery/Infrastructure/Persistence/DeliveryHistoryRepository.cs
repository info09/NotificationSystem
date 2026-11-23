using Delivery.Application.Abstractions;
using Delivery.Domain.DeliveryLogs;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Infrastructure.Persistence;

/// <summary>
/// Repository truy vấn lịch sử gửi Thông báo.
/// </summary>
public sealed class DeliveryHistoryRepository : IDeliveryHistoryRepository
{
    private readonly DeliveryDbContext _dbContext;

    /// <summary>
    /// Khởi tạo repository với DbContext của module Delivery.
    /// </summary>
    /// <param name="dbContext">DbContext của module Delivery.</param>
    public DeliveryHistoryRepository(DeliveryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Lấy danh sách Nhật ký gửi thông báo theo mã định danh Thông báo.
    /// </summary>
    /// <param name="notificationId">Mã định danh Thông báo.</param>
    /// <param name="fromUtc">Mốc thời gian sớm nhất (UTC).</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Danh sách Nhật ký gửi thông báo.</returns>
    public async Task<IReadOnlyList<NotificationDeliveryLog>> GetLogsByNotificationIdAsync(
        Guid notificationId,
        DateTime fromUtc,
        CancellationToken cancellationToken)
    {
        return await _dbContext.NotificationDeliveryLogs
            .AsNoTracking()
            .Where(x => x.NotificationId == notificationId && x.CreatedAtUtc >= fromUtc)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Lấy danh sách Lần thử gửi theo mã định danh Thông báo có phân trang.
    /// </summary>
    /// <param name="notificationId">Mã định danh Thông báo.</param>
    /// <param name="fromUtc">Mốc thời gian sớm nhất (UTC).</param>
    /// <param name="channel">Kênh gửi cần lọc.</param>
    /// <param name="result">Kết quả cần lọc.</param>
    /// <param name="page">Số trang.</param>
    /// <param name="pageSize">Kích thước trang.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Danh sách Lần thử gửi và tổng số bản ghi.</returns>
    public async Task<(IReadOnlyList<DeliveryAttempt> Items, int TotalCount)> GetAttemptsByNotificationIdAsync(
        Guid notificationId,
        DateTime fromUtc,
        string? channel,
        string? result,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.DeliveryAttempts
            .AsNoTracking()
            .Where(x => x.NotificationId == notificationId && x.AttemptedAtUtc >= fromUtc);

        if (!string.IsNullOrWhiteSpace(channel))
        {
            query = query.Where(x => x.Channel == channel.Trim());
        }

        if (!string.IsNullOrWhiteSpace(result))
        {
            var normalizedResult = result.Trim();
            query = query.Where(x => x.Result.ToString() == normalizedResult);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.AttemptedAtUtc)
            .ThenByDescending(x => x.AttemptNumber)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
