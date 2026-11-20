using Administration.Infrastructure.Persistence;
using BuildingBlocks.Observability.Logging;
using Delivery.Infrastructure.Persistence;
using IdentityAccess.Infrastructure.Persistence;
using IntegrationMessaging.Infrastructure.Persistence;
using Notification.Infrastructure.Persistence;
using TemplateConfiguration.Infrastructure.Persistence;
using WorkerHost;

var builder = Host.CreateApplicationBuilder(args);
builder.UseStructuredLogging();

builder.Services.AddAdministrationPersistence(builder.Configuration);
builder.Services.AddDeliveryPersistence(builder.Configuration);
builder.Services.AddIdentityAccessPersistence(builder.Configuration);
builder.Services.AddIntegrationMessagingPersistence(builder.Configuration);
builder.Services.AddNotificationPersistence(builder.Configuration);
builder.Services.AddTemplateConfigurationPersistence(builder.Configuration);
builder.Services.AddHostedService<PlaceholderWorker>();

var host = builder.Build();
host.Run();
