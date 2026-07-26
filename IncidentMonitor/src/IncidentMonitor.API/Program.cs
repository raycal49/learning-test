using FluentValidation;
using IncidentMonitor.API.Validators;
using IncidentMonitor.Application.DTOs;
using IncidentMonitor.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Infrastructure (EF Core, repos)
builder.Services.AddInfrastructure(builder.Configuration);

// Validation
builder.Services.AddScoped<IValidator<IngestLogBatchRequest>,  IngestLogBatchRequestValidator>();
builder.Services.AddScoped<IValidator<IngestLogRequest>,       IngestLogRequestValidator>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();
app.Run();