using Delivery.Domain.InAppNotifications;

namespace UnitTests.Delivery;

/// <summary>
/// Unit test cho thực thể Thông báo trong ứng dụng.
/// </summary>
public sealed class InAppNotificationTests
{
    [Fact]
    public void Create_WithValidData_ReturnsUnreadNotification()
    {
        var notificationId = Guid.NewGuid();
        var createdAtUtc = DateTime.UtcNow;

        var notification = InAppNotification.Create(
            Guid.NewGuid(),
            notificationId,
            "user-1",
            "Tiêu đề",
            "Nội dung",
            createdAtUtc);

        Assert.Equal(notificationId, notification.NotificationId);
        Assert.Equal("user-1", notification.RecipientUserId);
        Assert.Equal("Tiêu đề", notification.Title);
        Assert.Equal("Nội dung", notification.Content);
        Assert.Equal(ReadStatus.Unread, notification.ReadStatus);
        Assert.Equal(createdAtUtc, notification.CreatedAtUtc);
        Assert.Null(notification.ReadAtUtc);
    }

    [Fact]
    public void Create_WithEmptyNotificationId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            InAppNotification.Create(Guid.NewGuid(), Guid.Empty, "user-1", "T", "C"));
    }

    [Fact]
    public void Create_WithEmptyRecipientUserId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            InAppNotification.Create(Guid.NewGuid(), Guid.NewGuid(), " ", "T", "C"));
    }

    [Fact]
    public void MarkAsRead_WhenUnread_SetsReadStatusAndTimestamp()
    {
        var notification = InAppNotification.Create(Guid.NewGuid(), Guid.NewGuid(), "user-1", "T", "C");
        var readAt = DateTime.UtcNow;

        notification.MarkAsRead(readAt);

        Assert.Equal(ReadStatus.Read, notification.ReadStatus);
        Assert.Equal(readAt, notification.ReadAtUtc);
    }

    [Fact]
    public void MarkAsRead_WhenAlreadyRead_IsIdempotent()
    {
        var notification = InAppNotification.Create(Guid.NewGuid(), Guid.NewGuid(), "user-1", "T", "C");
        var firstReadAt = DateTime.UtcNow.AddMinutes(-5);
        notification.MarkAsRead(firstReadAt);

        notification.MarkAsRead(DateTime.UtcNow);

        Assert.Equal(ReadStatus.Read, notification.ReadStatus);
        Assert.Equal(firstReadAt, notification.ReadAtUtc);
    }
}
