using IntegrationMessaging.Domain;

namespace UnitTests.IntegrationMessaging;

/// <summary>
/// Unit test cho thực thể OutboxMessage.
/// </summary>
public sealed class OutboxMessageTests
{
    [Fact]
    public void CreatePending_WithValidData_SetsRetryFields()
    {
        var occurredAtUtc = new DateTime(2026, 4, 3, 10, 0, 0, DateTimeKind.Utc);

        var message = OutboxMessage.CreatePending(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "NotificationCreated",
            """{"channel":"Email"}""",
            occurredAtUtc);

        Assert.Equal(OutboxMessageStatus.Pending, message.Status);
        Assert.Equal(0, message.RetryCount);
        Assert.Equal(occurredAtUtc, message.NextAttemptAtUtc);
        Assert.Null(message.LastError);
        Assert.Null(message.ProcessedAtUtc);
    }

    [Fact]
    public void MarkPublished_SetsPublishedStatusAndClearsError()
    {
        var message = OutboxMessage.CreatePending(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "NotificationCreated",
            """{"channel":"Email"}""",
            DateTime.UtcNow);
        message.MarkPublishFailed("temporary", DateTime.UtcNow, DateTime.UtcNow.AddSeconds(5), maxRetryCount: 3);
        var processedAtUtc = DateTime.UtcNow.AddMinutes(1);

        message.MarkPublished(processedAtUtc);

        Assert.Equal(OutboxMessageStatus.Published, message.Status);
        Assert.Equal(processedAtUtc, message.ProcessedAtUtc);
        Assert.Null(message.LastError);
    }

    [Fact]
    public void MarkPublishFailed_WhenMaxRetryReached_SetsFailedStatus()
    {
        var message = OutboxMessage.CreatePending(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "NotificationCreated",
            """{"channel":"Email"}""",
            DateTime.UtcNow);
        var attemptedAtUtc = DateTime.UtcNow;

        message.MarkPublishFailed("fatal", attemptedAtUtc, attemptedAtUtc.AddSeconds(5), maxRetryCount: 1);

        Assert.Equal(OutboxMessageStatus.Failed, message.Status);
        Assert.Equal(1, message.RetryCount);
        Assert.Equal("fatal", message.LastError);
    }
}
