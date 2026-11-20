using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace E2ETests.Observability;

public sealed class HealthAndCorrelationE2ETests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public HealthAndCorrelationE2ETests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting(
                "ConnectionStrings:Database",
                "Host=localhost;Port=5432;Database=notification_test;Username=postgres;Password=postgres");
            builder.UseSetting("JWT_ISSUER", "e2e-issuer");
            builder.UseSetting("JWT_AUDIENCE", "e2e-audience");
            builder.UseSetting("JWT_SIGNING_KEY", "e2e-test-signing-key-min-32-chars-000001");
            builder.UseSetting("OpenTelemetry:Enabled", "true");
            builder.UseSetting("OpenTelemetry:Tracing:Enabled", "true");
            builder.UseSetting("OpenTelemetry:Metrics:Enabled", "true");
            builder.UseSetting("OpenTelemetry:Exporter:Console:Enabled", "false");
            builder.ConfigureTestServices(services =>
            {
                services.PostConfigure<HealthCheckServiceOptions>(options =>
                {
                    options.Registrations.Clear();
                    options.Registrations.Add(new HealthCheckRegistration(
                        "ready-stub",
                        new AlwaysHealthyHealthCheck(),
                        HealthStatus.Unhealthy,
                        tags: ["ready"]));
                });
            });
        }).CreateClient();
    }

    [Fact]
    public async Task LivenessEndpoint_WhenCalled_ReturnsOkWithCorrelationId()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.TryGetValues("X-Correlation-Id", out var values));
        Assert.False(string.IsNullOrWhiteSpace(Assert.Single(values)));
    }

    [Fact]
    public async Task ReadyEndpoint_WhenDependenciesHealthy_ReturnsOkWithCorrelationId()
    {
        var response = await _client.GetAsync("/health/ready");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.TryGetValues("X-Correlation-Id", out var values));
        Assert.False(string.IsNullOrWhiteSpace(Assert.Single(values)));
    }

    private sealed class AlwaysHealthyHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(HealthCheckResult.Healthy());
    }
}
