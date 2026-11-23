using IntegrationMessaging.Domain;

namespace UnitTests.IntegrationMessaging;

/// <summary>
/// Unit test cho thực thể InboxMessage.
/// </summary>
public sealed class InboxMessageTests
{
    [Fact]
    public void Create_WithValidData_ReturnsInboxMessage()
    {
        var id = Guid.NewGuid();
        var messageId = Guid.NewGuid();
        var processedAtUtc = DateTime.UtcNow;

        var inboxMessage = InboxMessage.Create(id, messageId, "NotificationWorker", "NotificationCreated", processedAtUtc);

        Assert.Equal(id, inboxMessage.Id);
        Assert.Equal(messageId, inboxMessage.MessageId);
        Assert.Equal("NotificationWorker", inboxMessage.ConsumerName);
        Assert.Equal("NotificationCreated", inboxMessage.EventType);
        Assert.Equal(processedAtUtc, inboxMessage.ProcessedAtUtc);
    }

    [Fact]
    public void Create_WithEmptyMessageId_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            InboxMessage.Create(Guid.NewGuid(), Guid.Empty, "NotificationWorker", "NotificationCreated", DateTime.UtcNow));

        Assert.Equal("messageId", exception.ParamName);
    }

    [Fact]
    public void Create_WithEmptyConsumerName_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            InboxMessage.Create(Guid.NewGuid(), Guid.NewGuid(), "", "NotificationCreated", DateTime.UtcNow));

        Assert.Equal("consumerName", exception.ParamName);
    }
}
