using IntegrationMessaging.Domain;
using Worker;

namespace UnitTests.IntegrationMessaging;

/// <summary>
/// Unit test cho Outbox Publisher.
/// </summary>
public sealed class OutboxPublisherBackgroundServiceTests
{
    [Fact]
    public void ResolveRoutingKey_WithEmailChannel_ReturnsEmailRoutingKey()
    {
        var message = OutboxMessage.CreatePending(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "NotificationCreated",
            """{"channel":"Email","notificationId":"00000000-0000-0000-0000-000000000001"}""",
            DateTime.UtcNow);

        var routingKey = OutboxPublisherBackgroundService.ResolveRoutingKey(message);

        Assert.Equal("notification.email.created", routingKey);
    }

    [Fact]
    public void ResolveRoutingKey_WithInAppChannel_ReturnsInAppRoutingKey()
    {
        var message = OutboxMessage.CreatePending(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "NotificationCreated",
            """{"channel":"InApp","notificationId":"00000000-0000-0000-0000-000000000001"}""",
            DateTime.UtcNow);

        var routingKey = OutboxPublisherBackgroundService.ResolveRoutingKey(message);

        Assert.Equal("notification.inapp.created", routingKey);
    }

    [Fact]
    public void ResolveRoutingKey_WithUnknownChannel_ReturnsNull()
    {
        var message = OutboxMessage.CreatePending(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "NotificationCreated",
            """{"channel":"Sms"}""",
            DateTime.UtcNow);

        var routingKey = OutboxPublisherBackgroundService.ResolveRoutingKey(message);

        Assert.Null(routingKey);
    }

    [Fact]
    public void ResolveRoutingKey_WithInvalidPayload_ReturnsNull()
    {
        var message = OutboxMessage.CreatePending(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "NotificationCreated",
            "not-json",
            DateTime.UtcNow);

        var routingKey = OutboxPublisherBackgroundService.ResolveRoutingKey(message);

        Assert.Null(routingKey);
    }
}
