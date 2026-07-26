using FluentAssertions;
using IncidentMonitor.Application.Services;
using IncidentMonitor.Domain.Entities;
using IncidentMonitor.Domain.Enums;

namespace IncidentMonitor.UnitTests;

public sealed class AnomalyDetectorTests
{
    private readonly AnomalyDetector _detector = new();

    [Fact]
    public void Analyze_WithNoLogs_ReturnsNoAnomaly()
    {
        var result = _detector.Analyze("payment-service", new List<LogEntry>());

        result.IsAnomaly.Should().BeFalse();
        result.ErrorCount.Should().Be(0);
    }

    [Fact]
    public void Analyze_WithFewErrors_BelowThreshold_ReturnsNoAnomaly()
    {
        // Only 1 error — below MinErrorsForDetection of 3
        var logs = new List<LogEntry>
        {
            MakeLog(LogLevel.Error, DateTimeOffset.UtcNow.AddMinutes(-1)),
            MakeLog(LogLevel.Info,  DateTimeOffset.UtcNow.AddMinutes(-2)),
            MakeLog(LogLevel.Info,  DateTimeOffset.UtcNow.AddMinutes(-3)),
        };

        var result = _detector.Analyze("payment-service", logs);

        result.IsAnomaly.Should().BeFalse();
    }

    [Fact]
    public void Analyze_WithErrorSpike_ReturnsAnomaly()
    {
        var now  = new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var logs = new List<LogEntry>();

        // Give the baseline some variance: alternate between 0 and 2 errors per bucket
        // This gives mean=1, stdDev>0, so the spike of 15 will produce a high z-score
        for (var i = 0; i < 11; i++)
        {
            var bucketEnd   = now.AddMinutes(-((11 - i) * 5));
            var bucketStart = bucketEnd.AddMinutes(-5);
            var midpoint    = bucketStart.AddMinutes(2);

            // Even buckets get 2 errors, odd buckets get 0 — gives variance
            if (i % 2 == 0)
            {
                logs.Add(MakeLog(LogLevel.Error, midpoint));
                logs.Add(MakeLog(LogLevel.Error, midpoint.AddSeconds(30)));
            }
        }

        // 15 errors in current bucket (now-5min to now)
        for (var j = 0; j < 15; j++)
            logs.Add(MakeLog(LogLevel.Error, now.AddMinutes(-2)));

        var result = _detector.Analyze("payment-service", logs, now);

        result.IsAnomaly.Should().BeTrue();
        result.ZScore.Should().BeGreaterThan(2.0);
        result.ErrorCount.Should().Be(15);
    }

    [Fact]
    public void Analyze_ServiceName_IsPreservedInResult()
    {
        var result = _detector.Analyze("order-service", new List<LogEntry>());

        result.ServiceName.Should().Be("order-service");
    }

    [Fact]
    public void Analyze_ZScore_IsZero_WhenNoVarianceInBaseline()
    {
        // All buckets have 0 errors — stddev is 0, z-score should be 0
        var logs = new List<LogEntry>();
        var result = _detector.Analyze("payment-service", logs);

        result.ZScore.Should().Be(0);
    }

    private static LogEntry MakeLog(LogLevel level, DateTimeOffset timestamp) =>
        LogEntry.Create("payment-service", level, "test message", timestamp);
}