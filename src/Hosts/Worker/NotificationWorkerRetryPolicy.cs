using System.Net.Sockets;
using System.Text.Json;

namespace Worker;

/// <summary>
/// Chính sách phân loại lỗi và tính thời gian chờ retry cho NotificationWorker.
/// </summary>
internal static class NotificationWorkerRetryPolicy
{
    /// <summary>
    /// Số lần retry tối đa cho lỗi tạm thời.
    /// </summary>
    public const int MaxRetryCount = 3;

    private static readonly TimeSpan[] BaseDelays =
    [
        TimeSpan.FromSeconds(10),
        TimeSpan.FromSeconds(30),
        TimeSpan.FromSeconds(90)
    ];

    private static readonly TimeSpan MaxDelay = TimeSpan.FromMinutes(2);

    /// <summary>
    /// Xác định lỗi có thể retry hay không.
    /// </summary>
    /// <param name="exception">Ngoại lệ cần phân loại.</param>
    /// <returns><c>true</c> nếu lỗi tạm thời và có thể retry; ngược lại <c>false</c>.</returns>
    public static bool IsRetryable(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return exception switch
        {
            OperationCanceledException => false,
            ArgumentException => false,
            JsonException => false,
            FormatException => false,
            TimeoutException => true,
            HttpRequestException => true,
            IOException => true,
            SocketException => true,
            _ => exception.InnerException is not null && IsRetryable(exception.InnerException)
        };
    }

    /// <summary>
    /// Tính thời gian chờ cho lần retry.
    /// </summary>
    /// <param name="retryAttempt">Lần retry thứ mấy, bắt đầu từ 1.</param>
    /// <param name="random">Nguồn random dùng để tạo jitter.</param>
    /// <param name="baseDelays">Danh sách thời gian chờ cơ sở.</param>
    /// <param name="maxDelay">Thời gian chờ tối đa.</param>
    /// <returns>Thời gian chờ trước lần retry.</returns>
    public static TimeSpan GetDelay(
        int retryAttempt,
        Random? random = null,
        TimeSpan[]? baseDelays = null,
        TimeSpan? maxDelay = null)
    {
        if (retryAttempt < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(retryAttempt), "Retry attempt must be at least 1.");
        }

        var delays = baseDelays ?? BaseDelays;
        var limit = maxDelay ?? MaxDelay;
        var index = Math.Min(retryAttempt - 1, delays.Length - 1);
        var baseDelay = delays[index];
        var jitterMs = (random ?? Random.Shared).Next(0, 1000);
        var delay = baseDelay + TimeSpan.FromMilliseconds(jitterMs);

        return delay <= limit ? delay : limit;
    }
}
