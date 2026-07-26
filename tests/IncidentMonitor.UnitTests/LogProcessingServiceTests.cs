using FluentAssertions;
using IncidentMonitor.Application.Interfaces;
using IncidentMonitor.Application.Services;
using IncidentMonitor.Domain.Entities;
using IncidentMonitor.Domain.Enums;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace IncidentMonitor.UnitTests;

public sealed class LogProcessingServiceTests
{
    private readonly Mock<ILogRepository>      _logRepo      = new();
    private readonly Mock<IIncidentRepository> _incidentRepo = new();
    private readonly Mock<IAlertRepository>    _alertRepo    = new();
    private readonly AnomalyDetector           _detector     = new();
    private readonly LogProcessingService      _service;

    public LogProcessingServiceTests()
    {
        _incidentRepo
            .Setup(r => r.FindOpenByPatternAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Incident?)null);

        // Default setup for GetRecentByServiceAsync — returns empty list
        _logRepo
            .Setup(r => r.GetRecentByServiceAsync(
                It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<LogEntry>());

        var clusterer = new IncidentClusterer(_incidentRepo.Object);

        _service = new LogProcessingService(
            _logRepo.Object,
            _incidentRepo.Object,
            _alertRepo.Object,
            _detector,
            clusterer,
            NullLogger<LogProcessingService>.Instance);
    }

    [Fact]
    public async Task ProcessBatchAsync_PersistsAllLogs()
    {
        var logs = new List<LogEntry>
        {
            LogEntry.Create("payment-service", LogLevel.Info,  "Payment processed", DateTimeOffset.UtcNow),
            LogEntry.Create("payment-service", LogLevel.Error, "Gateway timeout",   DateTimeOffset.UtcNow)
        };

        await _service.ProcessBatchAsync(logs);

        _logRepo.Verify(r => r.AddRangeAsync(
            It.Is<IEnumerable<LogEntry>>(l => l.Count() == 2),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessBatchAsync_WithErrorLogs_RunsAnomalyDetection()
    {
        var logs = new List<LogEntry>
        {
            LogEntry.Create("auth-service", LogLevel.Error, "Auth failed", DateTimeOffset.UtcNow)
        };

        await _service.ProcessBatchAsync(logs);

        _logRepo.Verify(r => r.GetRecentByServiceAsync(
            "auth-service", It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessBatchAsync_WithSpike_CreatesAlert()
    {
        var now = new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

        // Baseline with variance: alternate 0 and 2 errors per bucket
        var historicalLogs = new List<LogEntry>();
        for (var i = 0; i < 11; i++)
        {
            var bucketEnd   = now.AddMinutes(-((11 - i) * 5));
            var bucketStart = bucketEnd.AddMinutes(-5);
            var midpoint    = bucketStart.AddMinutes(2);

            if (i % 2 == 0)
            {
                historicalLogs.Add(LogEntry.Create(
                    "payment-service", LogLevel.Error, "Gateway timeout", midpoint));
                historicalLogs.Add(LogEntry.Create(
                    "payment-service", LogLevel.Error, "Gateway timeout", midpoint.AddSeconds(30)));
            }
        }

        // 15 errors in current bucket
        var spikeLogs = Enumerable.Range(0, 15)
            .Select(_ => LogEntry.Create(
                "payment-service", LogLevel.Error,
                "Gateway timeout", now.AddMinutes(-2)))
            .ToList();

        _logRepo
            .Setup(r => r.GetRecentByServiceAsync(
                "payment-service", It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(historicalLogs.Concat(spikeLogs).ToList());

        await _service.ProcessBatchAsync(spikeLogs, now);

        _alertRepo.Verify(r => r.AddAsync(
            It.Is<Alert>(a => a.ServiceName == "payment-service"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessBatchAsync_WithNoErrors_DoesNotCreateAlert()
    {
        var logs = new List<LogEntry>
        {
            LogEntry.Create("order-service", LogLevel.Info, "Order processed", DateTimeOffset.UtcNow),
            LogEntry.Create("order-service", LogLevel.Warn, "Slow response",   DateTimeOffset.UtcNow)
        };

        await _service.ProcessBatchAsync(logs);

        _alertRepo.Verify(r => r.AddAsync(
            It.IsAny<Alert>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }
}