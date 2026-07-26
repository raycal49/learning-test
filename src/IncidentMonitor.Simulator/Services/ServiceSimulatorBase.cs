using System.Text;
using System.Text.Json;
using IncidentMonitor.Simulator.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IncidentMonitor.Simulator.Services;

public abstract class ServiceSimulatorBase : BackgroundService
{
    private readonly HttpClient _http;
    private readonly ILogger _logger;
    private readonly Random _random = new();

    protected abstract string ServiceName { get; }
    protected abstract string ApiUrl { get; }

    // Normal log interval range in milliseconds
    protected virtual int MinIntervalMs => 500;
    protected virtual int MaxIntervalMs => 2000;

    // How often to trigger an error spike (every N batches)
    protected virtual int SpikeEveryNBatches => 20;

    // How many errors in a spike
    protected virtual int SpikeErrorCount => 8;

    protected abstract IReadOnlyList<string> InfoMessages  { get; }
    protected abstract IReadOnlyList<string> WarnMessages  { get; }
    protected abstract IReadOnlyList<string> ErrorMessages { get; }

    private int _batchCount = 0;

    protected ServiceSimulatorBase(HttpClient http, ILogger logger)
    {
        _http   = http;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("{Service} simulator started", ServiceName);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var logs = _batchCount % SpikeEveryNBatches == 0 && _batchCount > 0
                    ? GenerateSpike()
                    : GenerateNormalBatch();

                await SendBatchAsync(logs, stoppingToken);
                _batchCount++;

                var delay = _random.Next(MinIntervalMs, MaxIntervalMs);
                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Service} simulator error", ServiceName);
                await Task.Delay(5000, stoppingToken);
            }
        }

        _logger.LogInformation("{Service} simulator stopped", ServiceName);
    }

    private List<LogPayload> GenerateNormalBatch()
    {
        var logs  = new List<LogPayload>();
        var count = _random.Next(1, 4);

        for (var i = 0; i < count; i++)
        {
            var level   = PickLevel();
            var message = PickMessage(level);

            logs.Add(new LogPayload(
                ServiceName,
                level,
                message,
                DateTimeOffset.UtcNow));
        }

        return logs;
    }

    private List<LogPayload> GenerateSpike()
    {
        _logger.LogWarning("{Service} generating error spike!", ServiceName);

        var logs = new List<LogPayload>();

        // A spike is mostly errors
        for (var i = 0; i < SpikeErrorCount; i++)
        {
            logs.Add(new LogPayload(
                ServiceName,
                "ERROR",
                ErrorMessages[_random.Next(ErrorMessages.Count)],
                DateTimeOffset.UtcNow));
        }

        // Mixed with a few normal logs
        for (var i = 0; i < 2; i++)
        {
            logs.Add(new LogPayload(
                ServiceName,
                "INFO",
                InfoMessages[_random.Next(InfoMessages.Count)],
                DateTimeOffset.UtcNow));
        }

        return logs;
    }

    private string PickLevel()
    {
        var roll = _random.Next(100);
        return roll switch
        {
            < 70 => "INFO",
            < 85 => "WARN",
            _    => "ERROR"
        };
    }

    private string PickMessage(string level) => level switch
    {
        "INFO"  => InfoMessages[_random.Next(InfoMessages.Count)],
        "WARN"  => WarnMessages[_random.Next(WarnMessages.Count)],
        "ERROR" => ErrorMessages[_random.Next(ErrorMessages.Count)],
        _       => "Unknown event"
    };

    private async Task SendBatchAsync(
        IReadOnlyList<LogPayload> logs,
        CancellationToken ct)
    {
        var batch   = new LogBatch(logs);
        var json    = JsonSerializer.Serialize(batch, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _http.PostAsync(ApiUrl, content, ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "{Service} batch rejected: {Status}",
                ServiceName, response.StatusCode);
        }
    }
}