using Administration.Infrastructure.Persistence;
using Api.HealthChecks;
using Delivery.Infrastructure.Persistence;
using IdentityAccess.Infrastructure.Persistence;
using IdentityAccess.Presentation;
using IntegrationMessaging.Infrastructure.Persistence;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Notification.Infrastructure.Persistence;
using TemplateConfiguration.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddIdentityAccessJwtBearerAuthentication(builder.Configuration);
builder.Services.AddIdentityAccessRbacAuthorization();
builder.Services.AddAdministrationPersistence(builder.Configuration);
builder.Services.AddDeliveryPersistence(builder.Configuration);
builder.Services.AddIdentityAccessPersistence(builder.Configuration);
builder.Services.AddIntegrationMessagingPersistence(builder.Configuration);
builder.Services.AddNotificationPersistence(builder.Configuration);
builder.Services.AddTemplateConfigurationPersistence(builder.Configuration);

builder.Services.AddSingleton<RabbitMqTcpHealthCheck>();
builder.Services.AddHealthChecks()
    .AddDbContextCheck<NotificationDbContext>("database", tags: ["ready"])
    .AddCheck<RabbitMqTcpHealthCheck>("rabbitmq", tags: ["ready"]);

var app = builder.Build();

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

app.Run();

public partial class Program { }
