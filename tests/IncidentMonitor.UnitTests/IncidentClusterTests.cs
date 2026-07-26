using FluentAssertions;
using IncidentMonitor.Application.Interfaces;
using IncidentMonitor.Application.Services;
using IncidentMonitor.Domain.Entities;
using IncidentMonitor.Domain.Enums;
using Moq;

namespace IncidentMonitor.UnitTests;

public sealed class IncidentClustererTests
{
    private readonly Mock<IIncidentRepository> _repoMock;
    private readonly IncidentClusterer _clusterer;

    public IncidentClustererTests()
    {
        _repoMock  = new Mock<IIncidentRepository>();
        _clusterer = new IncidentClusterer(_repoMock.Object);
    }

    [Fact]
    public async Task ClusterAsync_WithNoErrorLogs_DoesNotCreateIncident()
    {
        var logs = new List<LogEntry>
        {
            LogEntry.Create("auth-service", LogLevel.Info, "User logged in", DateTimeOffset.UtcNow),
            LogEntry.Create("auth-service", LogLevel.Warn, "Slow response",  DateTimeOffset.UtcNow)
        };

        await _clusterer.ClusterAsync(logs);

        _repoMock.Verify(r => r.AddAsync(
            It.IsAny<Incident>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ClusterAsync_WithNewErrorPattern_CreatesNewIncident()
    {
        _repoMock
            .Setup(r => r.FindOpenByPatternAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Incident?)null);

        var logs = new List<LogEntry>
        {
            LogEntry.Create("payment-service", LogLevel.Error,
                "Payment gateway timeout", DateTimeOffset.UtcNow)
        };

        await _clusterer.ClusterAsync(logs);

        _repoMock.Verify(r => r.AddAsync(
            It.Is<Incident>(i =>
                i.ServiceName     == "payment-service" &&
                i.OccurrenceCount == 1),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ClusterAsync_WithExistingIncident_UpdatesOccurrenceCount()
    {
        var existing = Incident.Open(
            "payment-service",
            "payment gateway timeout",
            DateTimeOffset.UtcNow.AddMinutes(-10));

        _repoMock
            .Setup(r => r.FindOpenByPatternAsync(
                "payment-service",
                "payment gateway timeout",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var logs = new List<LogEntry>
        {
            LogEntry.Create("payment-service", LogLevel.Error,
                "Payment gateway timeout", DateTimeOffset.UtcNow)
        };

        await _clusterer.ClusterAsync(logs);

        _repoMock.Verify(r => r.UpdateAsync(
            It.Is<Incident>(i => i.OccurrenceCount == 2),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ClusterAsync_NormalizesNumbers_GroupsSimilarMessages()
    {
        _repoMock
            .Setup(r => r.FindOpenByPatternAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Incident?)null);

        var logs = new List<LogEntry>
        {
            LogEntry.Create("auth-service", LogLevel.Error,
                "Token validation failed for user 123", DateTimeOffset.UtcNow),
            LogEntry.Create("auth-service", LogLevel.Error,
                "Token validation failed for user 456", DateTimeOffset.UtcNow)
        };

        await _clusterer.ClusterAsync(logs);

        // Both map to same pattern — only one incident created
        _repoMock.Verify(r => r.AddAsync(
            It.IsAny<Incident>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ClusterAsync_DifferentServices_CreatesSeparateIncidents()
    {
        _repoMock
            .Setup(r => r.FindOpenByPatternAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Incident?)null);

        var logs = new List<LogEntry>
        {
            LogEntry.Create("payment-service", LogLevel.Error,
                "Database connection refused", DateTimeOffset.UtcNow),
            LogEntry.Create("auth-service", LogLevel.Error,
                "Database connection refused", DateTimeOffset.UtcNow)
        };

        await _clusterer.ClusterAsync(logs);

        // Same message but different services = two separate incidents
        _repoMock.Verify(r => r.AddAsync(
            It.IsAny<Incident>(),
            It.IsAny<CancellationToken>()), Times.Exactly(2));
    }
}