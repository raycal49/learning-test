using IncidentMonitor.Application.Interfaces;
using IncidentMonitor.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IncidentMonitor.Infrastructure.Persistence.Repositories;

public sealed class IncidentRepository : IIncidentRepository
{
    private readonly AppDbContext _db;
    public IncidentRepository(AppDbContext db) => _db = db;

    public async Task<Incident?> FindOpenByPatternAsync(
        string serviceName, string pattern, CancellationToken ct = default) =>
        await _db.Incidents.FirstOrDefaultAsync(
            i => i.ServiceName == serviceName
              && i.MessagePattern == pattern
              && !i.IsResolved, ct);

    public async Task AddAsync(Incident incident, CancellationToken ct = default)
    {
        await _db.Incidents.AddAsync(incident, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Incident incident, CancellationToken ct = default)
    {
        _db.Incidents.Update(incident);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<Incident>> GetAllOpenAsync(CancellationToken ct = default) =>
        await _db.Incidents
            .Where(i => !i.IsResolved)
            .OrderByDescending(i => i.LastSeen)
            .AsNoTracking()
            .ToListAsync(ct);
}