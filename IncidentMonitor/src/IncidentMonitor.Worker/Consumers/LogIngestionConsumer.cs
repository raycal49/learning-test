using System.Text;
using System.Text.Json;
using IncidentMonitor.Application.DTOs;
using IncidentMonitor.Application.Services;
using IncidentMonitor.Domain.Entities;
using IncidentMonitor.Domain.Enums;
using IncidentMonitor.Infrastructure.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using DomainLogLevel = IncidentMonitor.Domain.Enums.LogLevel;

namespace IncidentMonitor.Worker.Consumers;

public sealed class LogIngestionConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMqConnectionFactory _connectionFactory;
    private readonly ILogger<LogIngestionConsumer> _logger;

    private const string QueueName = "logs.ingest";

    public LogIngestionConsumer(
        IServiceScopeFactory scopeFactory,
        RabbitMqConnectionFactory connectionFactory,
        ILogger<LogIngestionConsumer> logger)
    {
        _scopeFactory      = scopeFactory;
        _connectionFactory = connectionFactory;
        _logger            = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Log ingestion consumer starting...");

        var connection = await _connectionFactory.GetConnectionAsync(stoppingToken);
        var channel    = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(
            queue:      QueueName,
            durable:    true,
            exclusive:  false,
            autoDelete: false,
            arguments:  null,
            cancellationToken: stoppingToken);

        await channel.BasicQosAsync(
            prefetchSize:  0,
            prefetchCount: 1,
            global:        false,
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                var json  = Encoding.UTF8.GetString(ea.Body.Span);
                var batch = JsonSerializer.Deserialize<IngestLogBatchRequest>(json);

                if (batch is null)
                {
                    await channel.BasicNackAsync(ea.DeliveryTag, false, false,
                        cancellationToken: stoppingToken);
                    return;
                }

                var entries = batch.Logs.Select(r =>
                {
                    var level = Enum.Parse<DomainLogLevel>(r.Level, ignoreCase: true);
                    return LogEntry.Create(
                        r.ServiceName,
                        level,
                        r.Message,
                        r.Timestamp ?? DateTimeOffset.UtcNow);
                }).ToList();

                using var scope = _scopeFactory.CreateScope();
                var processor   = scope.ServiceProvider
                    .GetRequiredService<LogProcessingService>();

                await processor.ProcessBatchAsync(entries, stoppingToken);

                await channel.BasicAckAsync(ea.DeliveryTag, false,
                    cancellationToken: stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process log batch — requeueing");
                await channel.BasicNackAsync(ea.DeliveryTag, false, requeue: true,
                    cancellationToken: stoppingToken);
            }
        };

        await channel.BasicConsumeAsync(
            queue:    QueueName,
            autoAck:  false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}