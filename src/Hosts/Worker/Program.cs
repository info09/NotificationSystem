using BuildingBlocks.Observability.Logging;
using BuildingBlocks.Observability.OpenTelemetry;
using Worker;
using Worker.Modules;

var builder = Host.CreateApplicationBuilder(args);
builder.UseStructuredLogging();
builder.AddOpenTelemetryObservability();

builder.Services.AddModules(builder.Configuration);

builder.Services.AddHostedService<PlaceholderWorker>();

var host = builder.Build();
host.Run();
