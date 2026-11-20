namespace UnitTests.Notification.Domain;

using DomainNotification = global::Notification.Domain.Notification;
using NotificationChannel = global::Notification.Domain.NotificationChannel;
using NotificationStatus = global::Notification.Domain.NotificationStatus;
using Recipient = global::Notification.Domain.Recipient;

public sealed class NotificationTests
{
    [Fact]
    public void Create_WithContent_SetsPendingStatusAndProperties()
    {
        var recipient = new Recipient("user@example.com");

        var notification = DomainNotification.Create(
            id: Guid.NewGuid(),
            recipient: recipient,
            channel: NotificationChannel.Email,
            content: "Xin chao",
            notificationTemplateId: null,
            idempotencyScope: "integration-client-1",
            idempotencyKey: "idem-key-1");

        Assert.Equal(NotificationStatus.Pending, notification.Status);
        Assert.Equal(recipient, notification.Recipient);
        Assert.Equal(NotificationChannel.Email, notification.Channel);
        Assert.Equal("Xin chao", notification.Content);
        Assert.Null(notification.NotificationTemplateId);
        Assert.Equal("integration-client-1", notification.IdempotencyScope);
        Assert.Equal("idem-key-1", notification.IdempotencyKey);
    }

    [Fact]
    public void Create_WithTemplateReference_SetsPendingStatus()
    {
        var notification = DomainNotification.Create(
            id: Guid.NewGuid(),
            recipient: new Recipient("recipient-id"),
            channel: NotificationChannel.InApp,
            content: null,
            notificationTemplateId: Guid.NewGuid(),
            idempotencyScope: "integration-client-1",
            idempotencyKey: "idem-key-2");

        Assert.Equal(NotificationStatus.Pending, notification.Status);
        Assert.NotNull(notification.NotificationTemplateId);
    }

    [Fact]
    public void Create_WithoutContentAndTemplateReference_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => DomainNotification.Create(
            id: Guid.NewGuid(),
            recipient: new Recipient("user@example.com"),
            channel: NotificationChannel.Email,
            content: null,
            notificationTemplateId: null,
            idempotencyScope: "integration-client-1",
            idempotencyKey: "idem-key-3"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithoutIdempotencyScope_ThrowsArgumentException(string? scope)
    {
        Assert.Throws<ArgumentException>(() => DomainNotification.Create(
            id: Guid.NewGuid(),
            recipient: new Recipient("user@example.com"),
            channel: NotificationChannel.Email,
            content: "Xin chao",
            notificationTemplateId: null,
            idempotencyScope: scope!,
            idempotencyKey: "idem-key-4"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithoutIdempotencyKey_ThrowsArgumentException(string? key)
    {
        Assert.Throws<ArgumentException>(() => DomainNotification.Create(
            id: Guid.NewGuid(),
            recipient: new Recipient("user@example.com"),
            channel: NotificationChannel.Email,
            content: "Xin chao",
            notificationTemplateId: null,
            idempotencyScope: "integration-client-1",
            idempotencyKey: key!));
    }

    [Fact]
    public void MarkProcessing_WhenPending_ChangesStatusToProcessing()
    {
        var notification = CreatePendingNotification();

        notification.MarkProcessing();

        Assert.Equal(NotificationStatus.Processing, notification.Status);
    }

    [Fact]
    public void MarkSent_WhenProcessing_ChangesStatusToSent()
    {
        var notification = CreatePendingNotification();
        notification.MarkProcessing();

        notification.MarkSent();

        Assert.Equal(NotificationStatus.Sent, notification.Status);
    }

    [Fact]
    public void MarkFailed_WhenProcessing_ChangesStatusToFailed()
    {
        var notification = CreatePendingNotification();
        notification.MarkProcessing();

        notification.MarkFailed();

        Assert.Equal(NotificationStatus.Failed, notification.Status);
    }

    [Fact]
    public void MarkProcessing_WhenNotPending_ThrowsInvalidOperationException()
    {
        var notification = CreatePendingNotification();
        notification.MarkProcessing();

        Assert.Throws<InvalidOperationException>(() => notification.MarkProcessing());
    }

    [Fact]
    public void MarkSent_WhenNotProcessing_ThrowsInvalidOperationException()
    {
        var notification = CreatePendingNotification();

        Assert.Throws<InvalidOperationException>(() => notification.MarkSent());
    }

    [Fact]
    public void MarkFailed_WhenNotProcessing_ThrowsInvalidOperationException()
    {
        var notification = CreatePendingNotification();

        Assert.Throws<InvalidOperationException>(() => notification.MarkFailed());
    }

    private static DomainNotification CreatePendingNotification()
    {
        return DomainNotification.Create(
            id: Guid.NewGuid(),
            recipient: new Recipient("user@example.com"),
            channel: NotificationChannel.Email,
            content: "Noi dung",
            notificationTemplateId: null,
            idempotencyScope: "integration-client-1",
            idempotencyKey: Guid.NewGuid().ToString());
    }
}
