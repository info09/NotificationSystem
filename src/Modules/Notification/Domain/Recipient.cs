namespace Notification.Domain;

/// <summary>
/// Giá trị đối tượng đại diện cho người nhận Thông báo.
/// </summary>
public sealed record Recipient
{
    /// <summary>
    /// Khởi tạo người nhận với giá trị định danh.
    /// </summary>
    /// <param name="value">Giá trị người nhận.</param>
    public Recipient(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Recipient value is required.", nameof(value));
        }

        Value = value;
    }

    /// <summary>
    /// Giá trị định danh người nhận.
    /// </summary>
    public string Value { get; }
}
