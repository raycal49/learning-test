using IncidentMonitor.Domain.Entities;

namespace IncidentMonitor.Application.Interfaces;

public interface ILogRepository
{
    Task AddRangeAsync(IEnumerable<LogEntry> logs, CancellationToken ct = default);

    Task<IReadOnlyList<LogEntry>> GetRecentByServiceAsync(
        string serviceName, int windowMinutes, CancellationToken ct = default);

    Task<(IReadOnlyList<LogEntry> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize,
        string? serviceName = null,
        string? level = null,
        CancellationToken ct = default);

    Task<IReadOnlyList<LogEntry>> GetForTimeRangeAsync(
        DateTimeOffset from, DateTimeOffset to, CancellationToken ct = default);

    Task<int> GetTotalCountAsync(CancellationToken ct = default);
}