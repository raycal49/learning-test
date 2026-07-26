using IncidentMonitor.Application.Interfaces;
using IncidentMonitor.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace IncidentMonitor.Application.Services;

public sealed class LogProcessingService
{
    private readonly ILogRepository      _logRepository;
    private readonly IIncidentRepository _incidentRepository;
    private readonly IAlertRepository    _alertRepository;
    private readonly AnomalyDetector     _anomalyDetector;
    private readonly IncidentClusterer   _clusterer;
    private readonly ILogger<LogProcessingService> _logger;

    private const int AnomalyWindowMinutes = 60;

    public LogProcessingService(
        ILogRepository logRepository,
        IIncidentRepository incidentRepository,
        IAlertRepository alertRepository,
        AnomalyDetector anomalyDetector,
        IncidentClusterer clusterer,
        ILogger<LogProcessingService> logger)
    {
        _logRepository      = logRepository;
        _incidentRepository = incidentRepository;
        _alertRepository    = alertRepository;
        _anomalyDetector    = anomalyDetector;
        _clusterer          = clusterer;
        _logger             = logger;
    }

    public async Task ProcessBatchAsync(
        IReadOnlyList<LogEntry> entries,
        DateTimeOffset? now = null,
        CancellationToken ct = default)
    {
        await _logRepository.AddRangeAsync(entries, ct);
        _logger.LogInformation("Persisted {Count} log entries", entries.Count);

        await _clusterer.ClusterAsync(entries, ct);

        var services = entries.Select(e => e.ServiceName).Distinct();

        foreach (var service in services)
        {
            var recentLogs = await _logRepository.GetRecentByServiceAsync(
                service, AnomalyWindowMinutes, ct);

            var result = _anomalyDetector.Analyze(service, recentLogs, now);

            if (!result.IsAnomaly) continue;

            _logger.LogWarning(
                "Anomaly detected for {Service}: ZScore={ZScore}, ErrorCount={Count}",
                service, result.ZScore, result.ErrorCount);

            var alert = Alert.Trigger(
                service,
                $"Error spike detected — z-score {result.ZScore:F2} " +
                $"({result.ErrorCount} errors vs baseline mean of {result.Mean:F1})");

            await _alertRepository.AddAsync(alert, ct);
        }
    }
}