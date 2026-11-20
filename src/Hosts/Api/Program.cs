using IdentityAccess.Presentation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();
builder.Services.AddIdentityAccessJwtBearerAuthentication(builder.Configuration);
builder.Services.AddIdentityAccessRbacAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Ok(new { service = "NotificationSystem.Api", status = "running" }));
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");

app.Run();
