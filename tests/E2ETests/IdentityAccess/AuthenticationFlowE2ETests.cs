using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Net;
using System.Net.Http.Json;

namespace E2ETests.IdentityAccess;

public sealed class AuthenticationFlowE2ETests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AuthenticationFlowE2ETests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting(
                "ConnectionStrings:Database",
                "Host=localhost;Port=5432;Database=notification_test;Username=postgres;Password=postgres");
            builder.UseSetting("JWT_ISSUER", "e2e-issuer");
            builder.UseSetting("JWT_AUDIENCE", "e2e-audience");
            builder.UseSetting("JWT_SIGNING_KEY", "e2e-test-signing-key-min-32-chars-000001");
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
    public async Task RootEndpoint_AnonymousRequest_ReturnsOk()
    {
        var response = await _client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task HealthEndpoints_AnonymousRequest_ReturnsOk()
    {
        var healthResponse = await _client.GetAsync("/health");
        var readyResponse = await _client.GetAsync("/health/ready");

        Assert.Equal(HttpStatusCode.OK, healthResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, readyResponse.StatusCode);
    }

    [Fact]
    public async Task LoginEndpoint_WhenImplemented_IsNotNotFound()
    {
        // ST-P13-04 đã triển khai endpoint đăng nhập credential nội bộ.
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            userName = "admin",
            password = "password"
        });

        Assert.NotEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    private sealed class AlwaysHealthyHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(HealthCheckResult.Healthy());
    }
}
