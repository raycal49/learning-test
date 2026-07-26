using IncidentMonitor.Application.DTOs;
using IncidentMonitor.Application.Interfaces;
using IncidentMonitor.Domain.Enums;

namespace IncidentMonitor.Application.Services;

public sealed class DashboardService
{
    private readonly ILogRepository      _logRepo;
    private readonly IIncidentRepository _incidentRepo;
    private readonly IAlertRepository    _alertRepo;

    public DashboardService(
        ILogRepository logRepo,
        IIncidentRepository incidentRepo,
        IAlertRepository alertRepo)
    {
        _logRepo      = logRepo;
        _incidentRepo = incidentRepo;
        _alertRepo    = alertRepo;
    }

    public async Task<DashboardStatsDto> GetStatsAsync(CancellationToken ct = default)
    {
        var from = DateTimeOffset.UtcNow.AddHours(-1);
        var to   = DateTimeOffset.UtcNow;

        var recentLogs    = await _logRepo.GetForTimeRangeAsync(from, to, ct);
        var totalLogs     = await _logRepo.GetTotalCountAsync(ct);
        var totalIncidents = await _incidentRepo.GetTotalCountAsync(ct);
        var activeIncidents = await _incidentRepo.GetActiveCountAsync(ct);
        var totalAlerts   = await _alertRepo.GetTotalCountAsync(ct);

        // Service-level stats from the last hour
        var serviceStats = recentLogs
            .GroupBy(l => l.ServiceName)
            .Select(g =>
            {
                var total  = g.Count();
                var errors = g.Count(l => l.Level == LogLevel.Error);
                var warns  = g.Count(l => l.Level == LogLevel.Warn);
                var info   = g.Count(l => l.Level == LogLevel.Info);
                return new ServiceStatsDto(
                    g.Key, total, errors, warns, info,
                    total > 0 ? Math.Round((double)errors / total * 100, 1) : 0);
            })
            .OrderByDescending(s => s.ErrorRate)
            .ToList();

        // Volume bucketed into 5-minute intervals over the last hour
        var volumePoints = BuildVolumeTimeline(recentLogs, from, to);

        return new DashboardStatsDto(
            totalLogs,
            totalIncidents,
            totalAlerts,
            activeIncidents,
            serviceStats,
            volumePoints);
    }

    public async Task<PagedResult<LogEntryDto>> GetLogsAsync(
        int page, int pageSize,
        string? serviceName, string? level,
        CancellationToken ct = default)
    {
        var (items, total) = await _logRepo.GetPagedAsync(
            page, pageSize, serviceName, level, ct);

        var dtos = items.Select(l => new LogEntryDto(
            l.Id, l.ServiceName, l.Level.ToString(),
            l.Message, l.Timestamp)).ToList();

        return new PagedResult<LogEntryDto>(
            dtos, total, page, pageSize,
            (int)Math.Ceiling((double)total / pageSize));
    }

    public async Task<IReadOnlyList<IncidentDto>> GetIncidentsAsync(
        CancellationToken ct = default)
    {
        var incidents = await _incidentRepo.GetAllOpenAsync(ct);
        return incidents.Select(i => new IncidentDto(
            i.Id, i.ServiceName, i.MessagePattern,
            i.OccurrenceCount, i.FirstSeen, i.LastSeen,
            i.IsResolved)).ToList();
    }

    public async Task<IReadOnlyList<AlertDto>> GetAlertsAsync(
        int count, CancellationToken ct = default)
    {
        var alerts = await _alertRepo.GetRecentAsync(count, ct);
        return alerts.Select(a => new AlertDto(
            a.Id, a.ServiceName, a.Description,
            a.TriggeredAt, a.IsAcknowledged)).ToList();
    }

    private static IReadOnlyList<LogVolumePointDto> BuildVolumeTimeline(
        IEnumerable<IncidentMonitor.Domain.Entities.LogEntry> logs,
        DateTimeOffset from,
        DateTimeOffset to)
    {
        const int bucketMinutes = 5;
        var points = new List<LogVolumePointDto>();
        var current = from;

        while (current < to)
        {
            var bucketEnd = current.AddMinutes(bucketMinutes);
            var bucket    = logs
                .Where(l => l.Timestamp >= current && l.Timestamp < bucketEnd)
                .ToList();

            points.Add(new LogVolumePointDto(
                current,
                bucket.Count,
                bucket.Count(l => l.Level == LogLevel.Error),
                bucket.Count(l => l.Level == LogLevel.Warn),
                bucket.Count(l => l.Level == LogLevel.Info)));

            current = bucketEnd;
        }

        return points;
    }
}