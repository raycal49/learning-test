using IncidentMonitor.Domain.Entities;

namespace IncidentMonitor.Application.Interfaces;

public interface ILogRepository
{
    Task AddRangeAsync(IEnumerable<LogEntry> logs, CancellationToken ct = default);
    Task<IReadOnlyList<LogEntry>> GetRecentByServiceAsync(
        string serviceName, int windowMinutes, CancellationToken ct = default);
}