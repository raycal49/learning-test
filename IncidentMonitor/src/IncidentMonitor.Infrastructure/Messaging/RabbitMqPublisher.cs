using System.Text;
using System.Text.Json;
using IncidentMonitor.Application.Interfaces;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace IncidentMonitor.Infrastructure.Messaging;

public sealed class RabbitMqPublisher : IMessagePublisher
{
    private readonly RabbitMqConnectionFactory _connectionFactory;
    private readonly ILogger<RabbitMqPublisher> _logger;

    public RabbitMqPublisher(
        RabbitMqConnectionFactory connectionFactory,
        ILogger<RabbitMqPublisher> logger)
    {
        _connectionFactory = connectionFactory;
        _logger            = logger;
    }

    public async Task PublishAsync<T>(string queue, T message, CancellationToken ct = default)
    {
        var connection = await _connectionFactory.GetConnectionAsync(ct);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: ct);

        await channel.QueueDeclareAsync(
            queue:      queue,
            durable:    true,
            exclusive:  false,
            autoDelete: false,
            arguments:  null,
            cancellationToken: ct);

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

        var props = new BasicProperties
        {
            Persistent   = true,
            ContentType  = "application/json"
        };

        await channel.BasicPublishAsync(
            exchange:   string.Empty,
            routingKey: queue,
            mandatory:  false,
            basicProperties: props,
            body:       body,
            cancellationToken: ct);

        _logger.LogDebug("Published message to queue {Queue}", queue);
    }
}