namespace UnitTests.Notification.Domain;

using Recipient = global::Notification.Domain.Recipient;

public sealed class RecipientTests
{
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Constructor_WhenValueIsEmptyOrWhitespace_ThrowsArgumentException(string value)
    {
        Assert.Throws<ArgumentException>(() => new Recipient(value));
    }

    [Fact]
    public void Constructor_WhenValueIsNull_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Recipient(null!));
    }

    [Fact]
    public void Constructor_WithValidValue_SetsValue()
    {
        var recipient = new Recipient("user@example.com");

        Assert.Equal("user@example.com", recipient.Value);
    }
}
