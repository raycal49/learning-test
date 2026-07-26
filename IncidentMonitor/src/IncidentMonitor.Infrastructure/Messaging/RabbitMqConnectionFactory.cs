using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace IncidentMonitor.Infrastructure.Messaging;

public sealed class RabbitMqConnectionFactory : IAsyncDisposable
{
    private readonly RabbitMqSettings _settings;
    private readonly ILogger<RabbitMqConnectionFactory> _logger;
    private IConnection? _connection;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public RabbitMqConnectionFactory(
        IOptions<RabbitMqSettings> settings,
        ILogger<RabbitMqConnectionFactory> logger)
    {
        _settings = settings.Value;
        _logger   = logger;
    }

    public async Task<IConnection> GetConnectionAsync(CancellationToken ct = default)
    {
        if (_connection is { IsOpen: true })
            return _connection;

        await _lock.WaitAsync(ct);
        try
        {
            if (_connection is { IsOpen: true })
                return _connection;

            var factory = new ConnectionFactory
            {
                HostName    = _settings.Host,
                Port        = _settings.Port,
                UserName    = _settings.User,
                Password    = _settings.Pass,
                VirtualHost = _settings.VHost,
                AutomaticRecoveryEnabled = true
            };

            _connection = await factory.CreateConnectionAsync(ct);
            _logger.LogInformation("RabbitMQ connection established to {Host}", _settings.Host);
            return _connection;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
            await _connection.DisposeAsync();
    }
}