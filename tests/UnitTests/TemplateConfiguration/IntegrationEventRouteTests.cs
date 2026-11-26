using TemplateConfiguration.Domain.IntegrationEventRoutes;

namespace UnitTests.TemplateConfiguration;

/// <summary>
/// Unit test cho thực thể Định tuyến sự kiện tích hợp.
/// </summary>
public sealed class IntegrationEventRouteTests
{
    /// <summary>
    /// Kiểm tra tạo định tuyến hợp lệ.
    /// </summary>
    [Fact]
    public void Create_WithValidData_ReturnsRoute()
    {
        var route = IntegrationEventRoute.Create(
            Guid.NewGuid(),
            "OrderCreated",
            Guid.NewGuid(),
            "Email",
            "Mô tả",
            true);

        Assert.Equal("OrderCreated", route.EventType);
        Assert.Equal("Email", route.Channel);
        Assert.True(route.IsActive);
    }

    /// <summary>
    /// Kiểm tra tạo định tuyến thiếu mapping gây lỗi.
    /// </summary>
    [Fact]
    public void Create_WithoutMapping_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            IntegrationEventRoute.Create(Guid.NewGuid(), "OrderCreated", null, null, null, true));
    }
}
