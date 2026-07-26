using IncidentMonitor.Application.Interfaces;
using IncidentMonitor.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IncidentMonitor.Infrastructure.Persistence.Repositories;

public sealed class AlertRepository : IAlertRepository
{
    private readonly AppDbContext _db;
    public AlertRepository(AppDbContext db) => _db = db;

    public async Task AddAsync(Alert alert, CancellationToken ct = default)
    {
        await _db.Alerts.AddAsync(alert, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<Alert>> GetRecentAsync(int count, CancellationToken ct = default) =>
        await _db.Alerts
            .OrderByDescending(a => a.TriggeredAt)
            .Take(count)
            .AsNoTracking()
            .ToListAsync(ct);
}