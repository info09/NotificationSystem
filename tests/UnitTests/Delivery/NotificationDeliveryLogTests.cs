using Delivery.Domain.DeliveryLogs;

namespace UnitTests.Delivery;

/// <summary>
/// Unit test cho thực thể Nhật ký gửi thông báo.
/// </summary>
public sealed class NotificationDeliveryLogTests
{
    [Fact]
    public void Create_WithValidData_ReturnsLog()
    {
        var notificationId = Guid.NewGuid();
        var createdAtUtc = DateTime.UtcNow;

        var log = NotificationDeliveryLog.Create(
            Guid.NewGuid(),
            notificationId,
            "Email",
            DeliveryAttemptResult.Success,
            null,
            0,
            "corr-1",
            createdAtUtc);

        Assert.Equal(notificationId, log.NotificationId);
        Assert.Equal("Email", log.Channel);
        Assert.Equal(DeliveryAttemptResult.Success, log.Result);
        Assert.Null(log.ErrorMessage);
        Assert.Equal(0, log.RetryCount);
        Assert.Equal("corr-1", log.CorrelationId);
        Assert.Equal(createdAtUtc, log.CreatedAtUtc);
    }

    [Fact]
    public void Create_WithEmptyNotificationId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            NotificationDeliveryLog.Create(Guid.NewGuid(), Guid.Empty, "Email", DeliveryAttemptResult.Failed, "err", 1, "corr"));
    }

    [Fact]
    public void Create_WithNegativeRetryCount_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            NotificationDeliveryLog.Create(Guid.NewGuid(), Guid.NewGuid(), "Email", DeliveryAttemptResult.Failed, "err", -1, "corr"));
    }

    [Fact]
    public void Create_WithEmptyChannel_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            NotificationDeliveryLog.Create(Guid.NewGuid(), Guid.NewGuid(), "", DeliveryAttemptResult.Success, null, 0, "corr"));
    }

    [Fact]
    public void Update_WithValidData_UpdatesFields()
    {
        var log = NotificationDeliveryLog.Create(Guid.NewGuid(), Guid.NewGuid(), "Email", DeliveryAttemptResult.Failed, "err", 0, "corr-1");

        log.Update(DeliveryAttemptResult.Success, null, 1, "corr-2");

        Assert.Equal(DeliveryAttemptResult.Success, log.Result);
        Assert.Null(log.ErrorMessage);
        Assert.Equal(1, log.RetryCount);
        Assert.Equal("corr-2", log.CorrelationId);
    }

    [Fact]
    public void Update_WithNegativeRetryCount_ThrowsArgumentOutOfRangeException()
    {
        var log = NotificationDeliveryLog.Create(Guid.NewGuid(), Guid.NewGuid(), "Email", DeliveryAttemptResult.Failed, "err", 0, "corr-1");

        Assert.Throws<ArgumentOutOfRangeException>(() => log.Update(DeliveryAttemptResult.Failed, "err", -1, "corr-1"));
    }
}
