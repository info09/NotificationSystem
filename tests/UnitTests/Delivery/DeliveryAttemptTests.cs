using Delivery.Domain.DeliveryLogs;

namespace UnitTests.Delivery;

/// <summary>
/// Unit test cho thực thể Lần thử gửi.
/// </summary>
public sealed class DeliveryAttemptTests
{
    [Fact]
    public void Create_WithValidData_ReturnsDeliveryAttempt()
    {
        var notificationId = Guid.NewGuid();
        var attemptedAtUtc = DateTime.UtcNow;

        var attempt = DeliveryAttempt.Create(
            Guid.NewGuid(),
            notificationId,
            "Email",
            1,
            DeliveryAttemptResult.Failed,
            "SMTP timeout",
            "corr-1",
            attemptedAtUtc);

        Assert.Equal(notificationId, attempt.NotificationId);
        Assert.Equal("Email", attempt.Channel);
        Assert.Equal(1, attempt.AttemptNumber);
        Assert.Equal(DeliveryAttemptResult.Failed, attempt.Result);
        Assert.Equal("SMTP timeout", attempt.ErrorMessage);
        Assert.Equal("corr-1", attempt.CorrelationId);
        Assert.Equal(attemptedAtUtc, attempt.AttemptedAtUtc);
    }

    [Fact]
    public void Create_WithEmptyNotificationId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            DeliveryAttempt.Create(Guid.NewGuid(), Guid.Empty, "Email", 1, DeliveryAttemptResult.Failed, "err", "corr"));
    }

    [Fact]
    public void Create_WithInvalidAttemptNumber_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            DeliveryAttempt.Create(Guid.NewGuid(), Guid.NewGuid(), "Email", 0, DeliveryAttemptResult.Failed, "err", "corr"));
    }

    [Fact]
    public void Create_WithEmptyChannel_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            DeliveryAttempt.Create(Guid.NewGuid(), Guid.NewGuid(), " ", 1, DeliveryAttemptResult.Success, null, "corr"));
    }
}
