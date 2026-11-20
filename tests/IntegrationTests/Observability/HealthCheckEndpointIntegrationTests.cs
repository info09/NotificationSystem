using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace IntegrationTests.Observability;

public sealed class HealthCheckEndpointIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public HealthCheckEndpointIntegrationTests(WebApplicationFactory<Program> factory)
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
        });
    }

    [Fact]
    public async Task Health_Liveness_ReturnsOk_WhenReadinessChecksAreUnhealthy()
    {
        using var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.PostConfigure<HealthCheckServiceOptions>(options =>
                {
                    options.Registrations.Clear();
                    options.Registrations.Add(new HealthCheckRegistration(
                        "ready-unhealthy",
                        new StaticHealthCheck(HealthCheckResult.Unhealthy("dependency unavailable")),
                        HealthStatus.Unhealthy,
                        tags: ["ready"]));
                });
            });
        }).CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task HealthReady_ReturnsServiceUnavailable_WhenReadinessChecksAreUnhealthy()
    {
        using var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.PostConfigure<HealthCheckServiceOptions>(options =>
                {
                    options.Registrations.Clear();
                    options.Registrations.Add(new HealthCheckRegistration(
                        "ready-unhealthy",
                        new StaticHealthCheck(HealthCheckResult.Unhealthy("dependency unavailable")),
                        HealthStatus.Unhealthy,
                        tags: ["ready"]));
                });
            });
        }).CreateClient();

        var response = await client.GetAsync("/health/ready");

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task HealthReady_ReturnsOk_WhenReadinessChecksAreHealthy()
    {
        using var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.PostConfigure<HealthCheckServiceOptions>(options =>
                {
                    options.Registrations.Clear();
                    options.Registrations.Add(new HealthCheckRegistration(
                        "ready-stub",
                        new StaticHealthCheck(HealthCheckResult.Healthy()),
                        HealthStatus.Unhealthy,
                        tags: ["ready"]));
                });
            });
        }).CreateClient();

        var response = await client.GetAsync("/health/ready");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private sealed class StaticHealthCheck(HealthCheckResult result) : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(result);
    }
}
