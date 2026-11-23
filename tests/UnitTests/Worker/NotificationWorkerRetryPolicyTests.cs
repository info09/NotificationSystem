using System.Net.Sockets;
using System.Text.Json;
using Worker;

namespace UnitTests.Worker;

/// <summary>
/// Unit test cho chính sách retry của NotificationWorker.
/// </summary>
public sealed class NotificationWorkerRetryPolicyTests
{
    [Fact]
    public void IsRetryable_WithTransientException_ReturnsTrue()
    {
        Assert.True(NotificationWorkerRetryPolicy.IsRetryable(new TimeoutException()));
        Assert.True(NotificationWorkerRetryPolicy.IsRetryable(new HttpRequestException()));
        Assert.True(NotificationWorkerRetryPolicy.IsRetryable(new IOException()));
        Assert.True(NotificationWorkerRetryPolicy.IsRetryable(new SocketException()));
    }

    [Fact]
    public void IsRetryable_WithNonRetryableException_ReturnsFalse()
    {
        Assert.False(NotificationWorkerRetryPolicy.IsRetryable(new ArgumentException("bad")));
        Assert.False(NotificationWorkerRetryPolicy.IsRetryable(new JsonException("bad")));
        Assert.False(NotificationWorkerRetryPolicy.IsRetryable(new FormatException("bad")));
        Assert.False(NotificationWorkerRetryPolicy.IsRetryable(new OperationCanceledException()));
    }

    [Fact]
    public void GetDelay_UsesExpectedBaseDelaysAndCap()
    {
        var random = new Random(0);

        var first = NotificationWorkerRetryPolicy.GetDelay(1, random, [TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(90)], TimeSpan.FromMinutes(2));
        var second = NotificationWorkerRetryPolicy.GetDelay(2, random, [TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(90)], TimeSpan.FromMinutes(2));
        var third = NotificationWorkerRetryPolicy.GetDelay(3, random, [TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(90)], TimeSpan.FromMinutes(2));

        Assert.InRange(first, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(11));
        Assert.InRange(second, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(31));
        Assert.InRange(third, TimeSpan.FromSeconds(90), TimeSpan.FromSeconds(91));
        Assert.Equal(3, NotificationWorkerRetryPolicy.MaxRetryCount);
    }
}
