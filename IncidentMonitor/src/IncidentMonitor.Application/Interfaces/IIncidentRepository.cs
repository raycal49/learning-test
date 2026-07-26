using IncidentMonitor.Domain.Entities;

namespace IncidentMonitor.Application.Interfaces;

public interface IIncidentRepository
{
    Task<Incident?> FindOpenByPatternAsync(
        string serviceName, string pattern, CancellationToken ct = default);
    Task AddAsync(Incident incident, CancellationToken ct = default);
    Task UpdateAsync(Incident incident, CancellationToken ct = default);
    Task<IReadOnlyList<Incident>> GetAllOpenAsync(CancellationToken ct = default);
}