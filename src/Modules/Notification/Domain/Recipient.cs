namespace Notification.Domain;

public sealed record Recipient
{
    public Recipient(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Recipient value is required.", nameof(value));
        }

        Value = value;
    }

    public string Value { get; }
}
