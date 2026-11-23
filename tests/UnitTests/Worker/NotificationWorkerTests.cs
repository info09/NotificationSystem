using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Worker;

namespace UnitTests.Worker;

/// <summary>
/// Unit test cho NotificationWorker.
/// </summary>
public sealed class NotificationWorkerTests
{
    [Fact]
    public void ResolveMessageId_WithValidMessageId_ReturnsGuid()
    {
        var messageId = Guid.NewGuid();
        var ea = CreateDeliveryEventArgs(messageId.ToString(), "{}"u8.ToArray());

        var result = NotificationWorker.ResolveMessageId(ea);

        Assert.Equal(messageId, result);
    }

    [Fact]
    public void ResolveMessageId_WithInvalidMessageId_ReturnsEmpty()
    {
        var ea = CreateDeliveryEventArgs("not-a-guid", "{}"u8.ToArray());

        var result = NotificationWorker.ResolveMessageId(ea);

        Assert.Equal(Guid.Empty, result);
    }

    private static BasicDeliverEventArgs CreateDeliveryEventArgs(string? messageId, byte[] body)
    {
        var properties = new BasicProperties
        {
            MessageId = messageId
        };

        return new BasicDeliverEventArgs(
            consumerTag: "ctag",
            deliveryTag: 1,
            redelivered: false,
            exchange: "notifications.topic",
            routingKey: "notification.email.created",
            properties: properties,
            body: body);
    }
}
