using Administration.Presentation;
using Api.HealthChecks;
using Api.Modules;
using BuildingBlocks.Observability;
using BuildingBlocks.Observability.Correlation;
using BuildingBlocks.Observability.Logging;
using BuildingBlocks.Observability.OpenTelemetry;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Notification.Infrastructure.Persistence;
using Notification.Presentation;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.UseStructuredLogging();
builder.AddOpenTelemetryObservability(instrumentAspNetCore: true);

builder.Services.AddModules(builder.Configuration);
builder.Services.AddOpenApi();

builder.Services.AddSingleton<RabbitMqTcpHealthCheck>();
builder.Services.AddHealthChecks()
    .AddDbContextCheck<NotificationDbContext>("database", tags: ["ready"])
    .AddCheck<RabbitMqTcpHealthCheck>("rabbitmq", tags: ["ready"]);

var app = builder.Build();

app.UseCorrelationId();
app.UseSerilogRequestLogging();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Ok(new { service = "NotificationSystem.Api", status = "running" }))
    .AllowAnonymous();
app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => false
}).AllowAnonymous();
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
}).AllowAnonymous();

app.MapOpenApi().AllowAnonymous();
app.MapScalarApiReference().AllowAnonymous();

app.MapNotificationEndpoints();
app.MapAdministrationEndpoints();

// Gọi Configure của mỗi module
foreach (var module in app.Services.GetServices<IModuleInitializer>())
{
    module.Configure(app.Services, app.Environment);
}

app.Run();

/// <summary>
/// Điểm vào của ứng dụng API, dùng cho cấu hình host và kiểm thử tích hợp.
/// </summary>
public partial class Program { }
