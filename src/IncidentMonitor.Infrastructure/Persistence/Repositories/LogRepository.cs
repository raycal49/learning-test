using IncidentMonitor.Application.Interfaces;
using IncidentMonitor.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IncidentMonitor.Infrastructure.Persistence.Repositories;

public sealed class LogRepository : ILogRepository
{
    private readonly AppDbContext _db;
    public LogRepository(AppDbContext db) => _db = db;

    public async Task AddRangeAsync(
        IEnumerable<LogEntry> logs, CancellationToken ct = default)
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

    public async Task<(IReadOnlyList<LogEntry> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize,
        string? serviceName = null,
        string? level = null,
        CancellationToken ct = default)
    {
        var query = _db.Logs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(serviceName))
            query = query.Where(l => l.ServiceName == serviceName);

        if (!string.IsNullOrWhiteSpace(level) &&
            Enum.TryParse<IncidentMonitor.Domain.Enums.LogLevel>(level, true, out var parsed))
            query = query.Where(l => l.Level == parsed);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(l => l.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<IReadOnlyList<LogEntry>> GetForTimeRangeAsync(
        DateTimeOffset from, DateTimeOffset to, CancellationToken ct = default) =>
        await _db.Logs
            .Where(l => l.Timestamp >= from && l.Timestamp <= to)
            .OrderBy(l => l.Timestamp)
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task<int> GetTotalCountAsync(CancellationToken ct = default) =>
        await _db.Logs.CountAsync(ct);
}