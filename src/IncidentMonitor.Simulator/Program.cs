using IncidentMonitor.Simulator.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHttpClient<PaymentServiceSimulator>();
builder.Services.AddHttpClient<AuthServiceSimulator>();
builder.Services.AddHttpClient<OrderServiceSimulator>();

builder.Services.AddHostedService<PaymentServiceSimulator>();
builder.Services.AddHostedService<AuthServiceSimulator>();
builder.Services.AddHostedService<OrderServiceSimulator>();

var host = builder.Build();
host.Run();