using System.Text.RegularExpressions;
using IncidentMonitor.Application.Interfaces;
using IncidentMonitor.Domain.Entities;
using IncidentMonitor.Domain.Enums;

namespace IncidentMonitor.Application.Services;

public sealed class IncidentClusterer
{
    private readonly IIncidentRepository _incidentRepository;

    public IncidentClusterer(IIncidentRepository incidentRepository)
    {
        _incidentRepository = incidentRepository;
    }

    public async Task ClusterAsync(
        IReadOnlyList<LogEntry> logs,
        CancellationToken ct = default)
    {
        // Only cluster error logs
        var errorLogs = logs
            .Where(l => l.Level == LogLevel.Error)
            .ToList();

        if (errorLogs.Count == 0) return;

        // Group by service + normalized pattern
        var groups = errorLogs
            .GroupBy(l => new
            {
                l.ServiceName,
                Pattern = NormalizeMessage(l.Message)
            });

        foreach (var group in groups)
        {
            var existing = await _incidentRepository.FindOpenByPatternAsync(
                group.Key.ServiceName,
                group.Key.Pattern,
                ct);

            if (existing is not null)
            {
                foreach (var _ in group)
                    existing.RecordOccurrence(DateTimeOffset.UtcNow);

                await _incidentRepository.UpdateAsync(existing, ct);
            }
            else
            {
                var incident = Incident.Open(
                    group.Key.ServiceName,
                    group.Key.Pattern,
                    group.First().Timestamp);

                // Record additional occurrences beyond the first
                foreach (var _ in group.Skip(1))
                    incident.RecordOccurrence(DateTimeOffset.UtcNow);

                await _incidentRepository.AddAsync(incident, ct);
            }
        }
    }

    /// <summary>
    /// Strips out dynamic values (IDs, numbers, IPs, GUIDs) so that
    /// "User 123 not found" and "User 456 not found" map to the same pattern.
    /// </summary>
    private static string NormalizeMessage(string message)
    {
        var normalized = Regex.Replace(
            message,
            @"[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}",
            "{guid}",
            RegexOptions.IgnoreCase);

        normalized = Regex.Replace(normalized, @"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b", "{ip}");
        normalized = Regex.Replace(normalized, @"\b\d+\b", "{n}");
        normalized = normalized.ToLowerInvariant().Trim();

        return normalized.Length > 500 ? normalized[..500] : normalized;
    }
}