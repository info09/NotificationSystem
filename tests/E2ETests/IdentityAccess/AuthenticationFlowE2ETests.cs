using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;

namespace E2ETests.IdentityAccess;

public sealed class AuthenticationFlowE2ETests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AuthenticationFlowE2ETests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("JWT_ISSUER", "e2e-issuer");
            builder.UseSetting("JWT_AUDIENCE", "e2e-audience");
            builder.UseSetting("JWT_SIGNING_KEY", "e2e-test-signing-key-min-32-chars-000001");
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
    public async Task LoginEndpoint_WhenNotImplemented_ReturnsNotFound()
    {
        // ST-P1-04 scaffolds AdminUI login flow calling POST /api/auth/login.
        // Backend credential login/token issuance is planned in Phase 13 (ST-P13-04).
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            userName = "admin",
            password = "password"
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
