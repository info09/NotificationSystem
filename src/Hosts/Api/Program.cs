using Administration.Infrastructure.Persistence;
using Delivery.Infrastructure.Persistence;
using IdentityAccess.Infrastructure.Persistence;
using IdentityAccess.Presentation;
using IntegrationMessaging.Infrastructure.Persistence;
using Notification.Infrastructure.Persistence;
using TemplateConfiguration.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();
builder.Services.AddIdentityAccessJwtBearerAuthentication(builder.Configuration);
builder.Services.AddIdentityAccessRbacAuthorization();
builder.Services.AddAdministrationPersistence(builder.Configuration);
builder.Services.AddDeliveryPersistence(builder.Configuration);
builder.Services.AddIdentityAccessPersistence(builder.Configuration);
builder.Services.AddIntegrationMessagingPersistence(builder.Configuration);
builder.Services.AddNotificationPersistence(builder.Configuration);
builder.Services.AddTemplateConfigurationPersistence(builder.Configuration);

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Ok(new { service = "NotificationSystem.Api", status = "running" }))
    .AllowAnonymous();
app.MapHealthChecks("/health")
    .AllowAnonymous();
app.MapHealthChecks("/health/ready")
    .AllowAnonymous();

app.Run();

public partial class Program { }
