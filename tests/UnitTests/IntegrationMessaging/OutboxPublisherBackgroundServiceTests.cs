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

    [Fact]
    public void CalculateNextAttemptAtUtc_WithIncreasingRetryCount_UsesExponentialBackoffWithCap()
    {
        var attemptedAtUtc = new DateTime(2026, 4, 3, 10, 0, 0, DateTimeKind.Utc);

        var firstRetry = OutboxPublisherBackgroundService.CalculateNextAttemptAtUtc(1, attemptedAtUtc);
        var secondRetry = OutboxPublisherBackgroundService.CalculateNextAttemptAtUtc(2, attemptedAtUtc);
        var cappedRetry = OutboxPublisherBackgroundService.CalculateNextAttemptAtUtc(20, attemptedAtUtc);

        Assert.Equal(attemptedAtUtc.AddSeconds(5), firstRetry);
        Assert.Equal(attemptedAtUtc.AddSeconds(10), secondRetry);
        Assert.Equal(attemptedAtUtc.AddMinutes(5), cappedRetry);
    }

    [Fact]
    public void MarkPublishFailed_WhenRetryLimitReached_MarksFailed()
    {
        var message = OutboxMessage.CreatePending(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "NotificationCreated",
            """{"channel":"Email"}""",
            DateTime.UtcNow);
        var attemptedAtUtc = DateTime.UtcNow;

        message.MarkPublishFailed("boom", attemptedAtUtc, attemptedAtUtc.AddSeconds(5), maxRetryCount: 1);

        Assert.Equal(OutboxMessageStatus.Failed, message.Status);
        Assert.Equal(1, message.RetryCount);
        Assert.Equal("boom", message.LastError);
        Assert.Equal(attemptedAtUtc, message.ProcessedAtUtc);
    }

    [Fact]
    public void MarkPublishFailed_WhenRetryLimitNotReached_KeepsPendingAndSchedulesNextAttempt()
    {
        var message = OutboxMessage.CreatePending(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "NotificationCreated",
            """{"channel":"Email"}""",
            DateTime.UtcNow);
        var attemptedAtUtc = DateTime.UtcNow;
        var nextAttemptAtUtc = attemptedAtUtc.AddSeconds(5);

        message.MarkPublishFailed("temporary", attemptedAtUtc, nextAttemptAtUtc, maxRetryCount: 3);

        Assert.Equal(OutboxMessageStatus.Pending, message.Status);
        Assert.Equal(1, message.RetryCount);
        Assert.Equal("temporary", message.LastError);
        Assert.Equal(nextAttemptAtUtc, message.NextAttemptAtUtc);
    }
}
