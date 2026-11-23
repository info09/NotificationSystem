using IntegrationMessaging.Infrastructure.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace UnitTests.IntegrationMessaging;

/// <summary>
/// Unit test cho cấu hình topology RabbitMQ MVP.
/// </summary>
public sealed class RabbitMqTopologyTests
{
    [Fact]
    public void TopologyNames_MatchMvpContract()
    {
        Assert.Equal("notifications.topic", RabbitMqTopologyNames.NotificationsExchange);
        Assert.Equal("notifications.email.q", RabbitMqTopologyNames.EmailQueue);
        Assert.Equal("notifications.inapp.q", RabbitMqTopologyNames.InAppQueue);
        Assert.Equal("notifications.email.dlq", RabbitMqTopologyNames.EmailDeadLetterQueue);
        Assert.Equal("notifications.inapp.dlq", RabbitMqTopologyNames.InAppDeadLetterQueue);
        Assert.Equal("notification.email.*", RabbitMqTopologyNames.EmailRoutingKeyPattern);
        Assert.Equal("notification.inapp.*", RabbitMqTopologyNames.InAppRoutingKeyPattern);
    }

    [Fact]
    public void AddIntegrationMessagingRabbitMq_BindsOptionsFromEnvironmentVariables()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["RABBITMQ_HOST"] = "rabbitmq",
                ["RABBITMQ_PORT"] = "5673",
                ["RABBITMQ_USER"] = "user",
                ["RABBITMQ_PASSWORD"] = "pass",
                ["RABBITMQ_VHOST"] = "/app"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddIntegrationMessagingRabbitMq(configuration);

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<RabbitMqOptions>>().Value;

        Assert.Equal("rabbitmq", options.Host);
        Assert.Equal(5673, options.Port);
        Assert.Equal("user", options.UserName);
        Assert.Equal("pass", options.Password);
        Assert.Equal("/app", options.VirtualHost);
    }

    [Fact]
    public void AddIntegrationMessagingRabbitMq_RegistersTopologyInitializer()
    {
        var configuration = new ConfigurationBuilder().Build();
        var services = new ServiceCollection();

        services.AddIntegrationMessagingRabbitMq(configuration);

        using var provider = services.BuildServiceProvider();
        var initializer = provider.GetRequiredService<IRabbitMqTopologyInitializer>();

        Assert.IsType<RabbitMqTopologyInitializer>(initializer);
    }
}
