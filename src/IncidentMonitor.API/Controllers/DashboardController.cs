using IncidentMonitor.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace IncidentMonitor.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class DashboardController : ControllerBase
{
    private readonly DashboardService _dashboard;

    public DashboardController(DashboardService dashboard)
        => _dashboard = dashboard;

    /// <summary>Overall stats for the dashboard header cards.</summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats(CancellationToken ct)
    {
        var stats = await _dashboard.GetStatsAsync(ct);
        return Ok(stats);
    }

    /// <summary>Paginated log entries with optional filtering.</summary>
    [HttpGet("logs")]
    public async Task<IActionResult> GetLogs(
        [FromQuery] int page        = 1,
        [FromQuery] int pageSize    = 50,
        [FromQuery] string? service = null,
        [FromQuery] string? level   = null,
        CancellationToken ct        = default)
    {
        if (page < 1) page = 1;
        if (pageSize is < 1 or > 200) pageSize = 50;

        var result = await _dashboard.GetLogsAsync(page, pageSize, service, level, ct);
        return Ok(result);
    }

    /// <summary>All open incidents ordered by last seen.</summary>
    [HttpGet("incidents")]
    public async Task<IActionResult> GetIncidents(CancellationToken ct)
    {
        var incidents = await _dashboard.GetIncidentsAsync(ct);
        return Ok(incidents);
    }

    /// <summary>Most recent alerts.</summary>
    [HttpGet("alerts")]
    public async Task<IActionResult> GetAlerts(
        [FromQuery] int count = 50,
        CancellationToken ct  = default)
    {
        if (count is < 1 or > 200) count = 50;
        var alerts = await _dashboard.GetAlertsAsync(count, ct);
        return Ok(alerts);
    }
}