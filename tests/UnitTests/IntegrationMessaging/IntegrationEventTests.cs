using IntegrationMessaging.Domain;

namespace UnitTests.IntegrationMessaging;

/// <summary>
/// Unit test cho thực thể Sự kiện tích hợp.
/// </summary>
public sealed class IntegrationEventTests
{
    /// <summary>
    /// Kiểm tra tạo bản ghi Sự kiện tích hợp hợp lệ.
    /// </summary>
    [Fact]
    public void Create_WithValidData_ReturnsIntegrationEvent()
    {
        var now = DateTime.UtcNow;
        var integrationEvent = IntegrationEvent.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "OrderCreated",
            now.AddMinutes(-1),
            "order-service",
            "idem-1",
            "1.0",
            "{\"orderId\":1}",
            now);

        Assert.Equal("OrderCreated", integrationEvent.EventType);
        Assert.Equal("order-service", integrationEvent.Source);
        Assert.Equal("1.0", integrationEvent.Version);
    }

    /// <summary>
    /// Kiểm tra tạo bản ghi thiếu MessageId gây lỗi.
    /// </summary>
    [Fact]
    public void Create_WithEmptyMessageId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            IntegrationEvent.Create(
                Guid.NewGuid(),
                Guid.Empty,
                Guid.NewGuid(),
                "OrderCreated",
                DateTime.UtcNow,
                "order-service",
                null,
                "1.0",
                "{}",
                DateTime.UtcNow));
    }
}
