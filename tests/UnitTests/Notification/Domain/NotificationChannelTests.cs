namespace UnitTests.Notification.Domain;

using NotificationChannel = global::Notification.Domain.NotificationChannel;

public sealed class NotificationChannelTests
{
    [Fact]
    public void NotificationChannel_HasExpectedValues()
    {
        Assert.Equal(0, (int)NotificationChannel.Email);
        Assert.Equal(1, (int)NotificationChannel.InApp);
    }
}
