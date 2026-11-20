using System.Net;
using System.Net.Http.Json;
using BuildingBlocks.Observability.Correlation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;

namespace IntegrationTests.Observability;

public sealed class CorrelationIdMiddlewareIntegrationTests
{
    [Fact]
    public async Task InvokeAsync_WhenCorrelationHeadersProvided_PropagatesValuesAndResponseHeader()
    {
        await using var app = await CreateTestAppAsync();
        using var client = app.GetTestClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/correlation");
        request.Headers.TryAddWithoutValidation(CorrelationIdConstants.CorrelationIdHeaderName, "corr-123");
        request.Headers.TryAddWithoutValidation(CorrelationIdConstants.MessageIdHeaderName, "msg-456");
        request.Headers.TryAddWithoutValidation(CorrelationIdConstants.NotificationIdHeaderName, "notif-789");

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.TryGetValues(CorrelationIdConstants.CorrelationIdHeaderName, out var correlationValues));
        Assert.Equal("corr-123", Assert.Single(correlationValues));

        var payload = await response.Content.ReadFromJsonAsync<CorrelationPayload>();
        Assert.NotNull(payload);
        Assert.Equal("corr-123", payload.CorrelationId);
        Assert.Equal("msg-456", payload.MessageId);
        Assert.Equal("notif-789", payload.NotificationId);
        Assert.False(string.IsNullOrWhiteSpace(payload.TraceId));
    }

    [Fact]
    public async Task InvokeAsync_WhenCorrelationIdMissing_GeneratesCorrelationId()
    {
        await using var app = await CreateTestAppAsync();
        using var client = app.GetTestClient();

        var response = await client.GetAsync("/correlation");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.TryGetValues(CorrelationIdConstants.CorrelationIdHeaderName, out var correlationValues));
        var correlationId = Assert.Single(correlationValues);
        Assert.False(string.IsNullOrWhiteSpace(correlationId));

        var payload = await response.Content.ReadFromJsonAsync<CorrelationPayload>();
        Assert.NotNull(payload);
        Assert.Equal(correlationId, payload.CorrelationId);
    }

    private static async Task<WebApplication> CreateTestAppAsync()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = "Development"
        });

        builder.WebHost.UseTestServer();
        var app = builder.Build();
        app.UseCorrelationId();
        app.MapGet("/correlation", (HttpContext context) => Results.Ok(new CorrelationPayload(
            context.Items[CorrelationIdConstants.CorrelationIdPropertyName]?.ToString(),
            context.Items[CorrelationIdConstants.MessageIdPropertyName]?.ToString(),
            context.Items[CorrelationIdConstants.NotificationIdPropertyName]?.ToString(),
            context.Items[CorrelationIdConstants.TraceIdPropertyName]?.ToString())));

        await app.StartAsync();
        return app;
    }

    private sealed record CorrelationPayload(
        string? CorrelationId,
        string? MessageId,
        string? NotificationId,
        string? TraceId);
}
