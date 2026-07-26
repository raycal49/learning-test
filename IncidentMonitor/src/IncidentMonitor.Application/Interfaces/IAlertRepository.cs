using IncidentMonitor.Domain.Entities;

namespace IncidentMonitor.Application.Interfaces;

public interface IAlertRepository
{
    Task AddAsync(Alert alert, CancellationToken ct = default);
    Task<IReadOnlyList<Alert>> GetRecentAsync(int count, CancellationToken ct = default);
}