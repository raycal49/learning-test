using IncidentMonitor.Application.Interfaces;
using IncidentMonitor.Application.Services;
using IncidentMonitor.Infrastructure.Messaging;
using IncidentMonitor.Infrastructure.Persistence;
using IncidentMonitor.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IncidentMonitor.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        services.AddDbContext<AppDbContext>(opts =>
            opts.UseNpgsql(
                configuration.GetConnectionString("Postgres"),
                npgsql => npgsql.MigrationsAssembly(
                    typeof(AppDbContext).Assembly.FullName)));

        // Repositories
        services.AddScoped<ILogRepository,      LogRepository>();
        services.AddScoped<IIncidentRepository, IncidentRepository>();
        services.AddScoped<IAlertRepository,    AlertRepository>();

        // RabbitMQ
        var rabbitSection = configuration.GetSection(RabbitMqSettings.SectionName);
        services.Configure<RabbitMqSettings>(opts =>
        {
            opts.Host  = rabbitSection["Host"]  ?? "localhost";
            opts.User  = rabbitSection["User"]  ?? "guest";
            opts.Pass  = rabbitSection["Pass"]  ?? "guest";
            opts.Port  = int.TryParse(rabbitSection["Port"], out var port) ? port : 5672;
            opts.VHost = rabbitSection["VHost"] ?? "/";
        });
        services.AddSingleton<RabbitMqConnectionFactory>();
        services.AddScoped<IMessagePublisher, RabbitMqPublisher>();

        // Application services
        services.AddScoped<AnomalyDetector>();
        services.AddScoped<IncidentClusterer>();
        services.AddScoped<LogProcessingService>();

        return services;
    }
}