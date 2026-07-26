using IncidentMonitor.Infrastructure;
using IncidentMonitor.Infrastructure.Messaging;
using IncidentMonitor.Worker.Consumers;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSingleton<RabbitMqConnectionFactory>();
builder.Services.AddHostedService<LogIngestionConsumer>();

var host = builder.Build();
host.Run();