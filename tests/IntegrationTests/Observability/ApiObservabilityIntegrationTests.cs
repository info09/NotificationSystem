using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace IntegrationTests.Observability;

public sealed class ApiObservabilityIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ApiObservabilityIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting(
                "ConnectionStrings:Database",
                "Host=localhost;Port=5432;Database=notification_test;Username=postgres;Password=postgres");
            builder.UseSetting("JWT_ISSUER", "test-issuer");
            builder.UseSetting("JWT_AUDIENCE", "test-audience");
            builder.UseSetting("JWT_SIGNING_KEY", "integration-test-signing-key-min-32-chars-0001");
            builder.UseSetting("OpenTelemetry:Enabled", "true");
            builder.UseSetting("OpenTelemetry:Tracing:Enabled", "true");
            builder.UseSetting("OpenTelemetry:Metrics:Enabled", "true");
            builder.UseSetting("OpenTelemetry:Exporter:Console:Enabled", "false");
        });
    }

    [Fact]
    public async Task RootEndpoint_WithObservabilityEnabled_ReturnsCorrelationIdHeader()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.TryGetValues("X-Correlation-Id", out var values));
        Assert.False(string.IsNullOrWhiteSpace(Assert.Single(values)));
    }

    [Fact]
    public async Task LivenessEndpoint_WithObservabilityEnabled_ReturnsOk()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.TryGetValues("X-Correlation-Id", out var values));
        Assert.False(string.IsNullOrWhiteSpace(Assert.Single(values)));
    }
}
