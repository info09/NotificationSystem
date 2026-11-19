var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new { service = "NotificationSystem.Api", status = "running" }));
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");

app.Run();
