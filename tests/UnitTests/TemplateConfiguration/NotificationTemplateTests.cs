using TemplateConfiguration.Domain.NotificationTemplates;

namespace UnitTests.TemplateConfiguration;

/// <summary>
/// Unit test cho thực thể Mẫu thông báo.
/// </summary>
public sealed class NotificationTemplateTests
{
    /// <summary>
    /// Kiểm tra tạo Mẫu thông báo hợp lệ.
    /// </summary>
    [Fact]
    public void Create_WithValidData_ReturnsTemplate()
    {
        var id = Guid.NewGuid();

        var template = NotificationTemplate.Create(
            id,
            "order-created",
            "Đơn hàng mới",
            "Mẫu email đơn hàng",
            "Email",
            "Chào {{name}}",
            "Nội dung {{orderId}}",
            true);

        Assert.Equal(id, template.Id);
        Assert.Equal("order-created", template.TemplateKey);
        Assert.Equal("Email", template.Channel);
        Assert.True(template.IsActive);
    }

    /// <summary>
    /// Kiểm tra tạo Mẫu thông báo thiếu TemplateKey sẽ lỗi.
    /// </summary>
    [Fact]
    public void Create_WithMissingTemplateKey_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            NotificationTemplate.Create(Guid.NewGuid(), "", "Name", null, "Email", null, "Body"));

        Assert.Equal("templateKey", exception.ParamName);
    }

    /// <summary>
    /// Kiểm tra cập nhật Mẫu thông báo.
    /// </summary>
    [Fact]
    public void Update_WithValidData_UpdatesFields()
    {
        var template = NotificationTemplate.Create(Guid.NewGuid(), "key-1", "Name", null, "Email", null, "Body");

        template.Update("Name 2", "Desc", "InApp", "Subject", "Body 2", false);

        Assert.Equal("Name 2", template.Name);
        Assert.Equal("InApp", template.Channel);
        Assert.Equal("Body 2", template.Body);
        Assert.False(template.IsActive);
    }
}
