using FluentValidation;
using IncidentMonitor.API.Validators;
using IncidentMonitor.Application.DTOs;
using IncidentMonitor.Application.Services;
using IncidentMonitor.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS — allow React dev server
builder.Services.AddCors(opts =>
    opts.AddPolicy("ReactDev", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()));

// Infrastructure (EF Core, repos, RabbitMQ)
builder.Services.AddInfrastructure(builder.Configuration);

// Application services
builder.Services.AddScoped<DashboardService>();

// Validation
builder.Services.AddScoped<IValidator<IngestLogBatchRequest>,  IngestLogBatchRequestValidator>();
builder.Services.AddScoped<IValidator<IngestLogRequest>,       IngestLogRequestValidator>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("ReactDev");
app.UseAuthorization();
app.MapControllers();
app.Run();

// make program accessible to integration tests
public partial class Program { }