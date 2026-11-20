using IdentityAccess.Presentation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();
builder.Services.AddIdentityAccessJwtBearerAuthentication(builder.Configuration);

var app = builder.Build();

app.UseAuthentication();

app.MapGet("/", () => Results.Ok(new { service = "NotificationSystem.Api", status = "running" }));
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");

app.Run();
