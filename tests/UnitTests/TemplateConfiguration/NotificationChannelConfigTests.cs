using TemplateConfiguration.Domain.NotificationChannelConfigs;

namespace UnitTests.TemplateConfiguration;

/// <summary>
/// Unit test cho thực thể Cấu hình kênh thông báo.
/// </summary>
public sealed class NotificationChannelConfigTests
{
    /// <summary>
    /// Kiểm tra tạo cấu hình hợp lệ.
    /// </summary>
    [Fact]
    public void Create_WithValidData_ReturnsConfig()
    {
        var config = NotificationChannelConfig.Create(Guid.NewGuid(), "Email", "Email", "Cấu hình email", true);

        Assert.Equal("Email", config.Channel);
        Assert.True(config.IsEnabled);
    }

    /// <summary>
    /// Kiểm tra cập nhật cấu hình.
    /// </summary>
    [Fact]
    public void Update_WithValidData_UpdatesFields()
    {
        var config = NotificationChannelConfig.Create(Guid.NewGuid(), "Email", "Email", null, true);

        config.Update("Email Channel", "Mô tả", false);

        Assert.Equal("Email Channel", config.DisplayName);
        Assert.False(config.IsEnabled);
    }
}
