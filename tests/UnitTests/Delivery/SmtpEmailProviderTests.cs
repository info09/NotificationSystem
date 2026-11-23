using Delivery.Domain.Email;
using Delivery.Infrastructure.Email;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace UnitTests.Delivery;

/// <summary>
/// Unit test cho Nhà cung cấp Email SMTP.
/// </summary>
public sealed class SmtpEmailProviderTests
{
    [Fact]
    public void AddDeliveryEmail_WithDefaults_UsesMailpitSettings()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();

        var services = new ServiceCollection();
        services.AddDeliveryEmail(configuration);
        using var provider = services.BuildServiceProvider();

        var options = provider.GetRequiredService<IOptions<SmtpOptions>>().Value;

        Assert.Equal("mailpit", options.Host);
        Assert.Equal(1025, options.Port);
        Assert.Equal("no-reply@notification.local", options.From);
        Assert.False(options.EnableSsl);
    }

    [Fact]
    public void AddDeliveryEmail_BindsOptionsFromEnvironmentVariables()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["SMTP_HOST"] = "mailpit-test",
                ["SMTP_PORT"] = "2525",
                ["SMTP_USERNAME"] = "user",
                ["SMTP_PASSWORD"] = "pass",
                ["SMTP_FROM"] = "noreply@test.local",
                ["SMTP_ENABLE_SSL"] = "true"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddDeliveryEmail(configuration);
        using var provider = services.BuildServiceProvider();

        var options = provider.GetRequiredService<IOptions<SmtpOptions>>().Value;

        Assert.Equal("mailpit-test", options.Host);
        Assert.Equal(2525, options.Port);
        Assert.Equal("user", options.Username);
        Assert.Equal("pass", options.Password);
        Assert.Equal("noreply@test.local", options.From);
        Assert.True(options.EnableSsl);
    }

    [Fact]
    public async Task SendAsync_WithMissingRecipient_ThrowsArgumentException()
    {
        var provider = new SmtpEmailProvider(Options.Create(new SmtpOptions()));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            provider.SendAsync(new EmailMessage("", "subject", "<p>hi</p>", "hi"), CancellationToken.None));
    }
}
