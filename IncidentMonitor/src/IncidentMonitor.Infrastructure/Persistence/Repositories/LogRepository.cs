using IncidentMonitor.Application.Interfaces;
using IncidentMonitor.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IncidentMonitor.Infrastructure.Persistence.Repositories;

public sealed class LogRepository : ILogRepository
{
    private readonly AppDbContext _db;
    public LogRepository(AppDbContext db) => _db = db;

    public async Task AddRangeAsync(IEnumerable<LogEntry> logs, CancellationToken ct = default)
    {
        await _db.Logs.AddRangeAsync(logs, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<LogEntry>> GetRecentByServiceAsync(
        string serviceName, int windowMinutes, CancellationToken ct = default)
    {
        var cutoff = DateTimeOffset.UtcNow.AddMinutes(-windowMinutes);
        return await _db.Logs
            .Where(l => l.ServiceName == serviceName && l.Timestamp >= cutoff)
            .OrderByDescending(l => l.Timestamp)
            .AsNoTracking()
            .ToListAsync(ct);
    }
}