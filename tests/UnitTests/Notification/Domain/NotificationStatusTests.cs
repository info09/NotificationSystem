namespace UnitTests.Notification.Domain;

using NotificationStatus = global::Notification.Domain.NotificationStatus;

public sealed class NotificationStatusTests
{
    [Fact]
    public void NotificationStatus_HasExpectedValues()
    {
        Assert.Equal(0, (int)NotificationStatus.Pending);
        Assert.Equal(1, (int)NotificationStatus.Processing);
        Assert.Equal(2, (int)NotificationStatus.Sent);
        Assert.Equal(3, (int)NotificationStatus.Failed);
    }
}
