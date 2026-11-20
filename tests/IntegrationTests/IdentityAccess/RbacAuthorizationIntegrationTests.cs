using IdentityAccess.Domain;
using IdentityAccess.Presentation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;

namespace IntegrationTests.IdentityAccess;

public sealed class RbacAuthorizationIntegrationTests
{
    private const string Issuer = "test-issuer";
    private const string Audience = "test-audience";
    private const string SigningKey = "integration-test-signing-key-min-32-chars-0001";

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_ReturnsUnauthorizedProblemDetails()
    {
        await using var app = await CreateTestAppAsync();
        using var client = app.GetTestClient();

        var response = await client.GetAsync("/protected/system-admin");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status401Unauthorized, problem.Status);
        Assert.Equal("Unauthorized", problem.Title);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithWrongRole_ReturnsForbiddenProblemDetails()
    {
        await using var app = await CreateTestAppAsync();
        using var client = app.GetTestClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/protected/system-admin");
        request.Headers.Authorization = new AuthenticationHeaderValue(
            JwtBearerDefaults.AuthenticationScheme,
            CreateToken(RbacRoles.BusinessOperator));

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status403Forbidden, problem.Status);
        Assert.Equal("Forbidden", problem.Title);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithCorrectRole_ReturnsOk()
    {
        await using var app = await CreateTestAppAsync();
        using var client = app.GetTestClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/protected/system-admin");
        request.Headers.Authorization = new AuthenticationHeaderValue(
            JwtBearerDefaults.AuthenticationScheme,
            CreateToken(RbacRoles.SystemAdministrator));

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AnonymousEndpoint_WithoutToken_ReturnsOk()
    {
        await using var app = await CreateTestAppAsync();
        using var client = app.GetTestClient();

        var response = await client.GetAsync("/public");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static async Task<WebApplication> CreateTestAppAsync()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = "Development"
        });

        builder.WebHost.UseTestServer();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["JWT_ISSUER"] = Issuer,
            ["JWT_AUDIENCE"] = Audience,
            ["JWT_SIGNING_KEY"] = SigningKey
        });

        builder.Services.AddIdentityAccessJwtBearerAuthentication(builder.Configuration);
        builder.Services.AddIdentityAccessRbacAuthorization();

        var app = builder.Build();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapGet("/public", () => Results.Ok(new { status = "ok" }))
            .AllowAnonymous();

        app.MapGet("/protected/system-admin", () => Results.Ok(new { status = "ok" }))
            .RequireAuthorization(RbacPolicies.SystemAdministrator);

        await app.StartAsync();
        return app;
    }

    private static string CreateToken(string role)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims:
            [
                new Claim(ClaimTypes.NameIdentifier, "test-user"),
                new Claim(ClaimTypes.Name, "test-user"),
                new Claim(ClaimTypes.Role, role)
            ],
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
