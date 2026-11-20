using Administration.Infrastructure.Persistence;
using Delivery.Infrastructure.Persistence;
using IdentityAccess.Infrastructure.Persistence;
using IntegrationMessaging.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Notification.Infrastructure.Persistence;
using TemplateConfiguration.Infrastructure.Persistence;

namespace IntegrationTests.Persistence;

public sealed class ApiPersistenceRegistrationIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ApiPersistenceRegistrationIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting(
                "ConnectionStrings:Database",
                "Host=localhost;Port=5432;Database=notification_test;Username=postgres;Password=postgres");
            builder.UseSetting("JWT_ISSUER", "test-issuer");
            builder.UseSetting("JWT_AUDIENCE", "test-audience");
            builder.UseSetting("JWT_SIGNING_KEY", "integration-test-signing-key-min-32-chars-0001");
        });
    }

    [Fact]
    public void ModuleDbContexts_AreRegisteredAndResolvable()
    {
        using var scope = _factory.Services.CreateScope();
        var services = scope.ServiceProvider;

        Assert.NotNull(services.GetRequiredService<NotificationDbContext>());
        Assert.NotNull(services.GetRequiredService<TemplateConfigurationDbContext>());
        Assert.NotNull(services.GetRequiredService<IntegrationMessagingDbContext>());
        Assert.NotNull(services.GetRequiredService<DeliveryDbContext>());
        Assert.NotNull(services.GetRequiredService<AdministrationDbContext>());
        Assert.NotNull(services.GetRequiredService<IdentityAccessDbContext>());
    }
}
