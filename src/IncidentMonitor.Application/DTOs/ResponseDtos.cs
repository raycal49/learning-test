namespace IncidentMonitor.Application.DTOs;

public sealed record LogEntryDto(
    Guid Id,
    string ServiceName,
    string Level,
    string Message,
    DateTimeOffset Timestamp
);

public sealed record IncidentDto(
    Guid Id,
    string ServiceName,
    string MessagePattern,
    int OccurrenceCount,
    DateTimeOffset FirstSeen,
    DateTimeOffset LastSeen,
    bool IsResolved
);

public sealed record AlertDto(
    Guid Id,
    string ServiceName,
    string Description,
    DateTimeOffset TriggeredAt,
    bool IsAcknowledged
);

public sealed record LogVolumePointDto(
    DateTimeOffset Timestamp,
    int Total,
    int Errors,
    int Warnings,
    int Info
);

public sealed record ServiceStatsDto(
    string ServiceName,
    int TotalLogs,
    int ErrorCount,
    int WarnCount,
    int InfoCount,
    double ErrorRate
);

public sealed record DashboardStatsDto(
    int TotalLogs,
    int TotalIncidents,
    int TotalAlerts,
    int ActiveIncidents,
    IReadOnlyList<ServiceStatsDto> ServiceStats,
    IReadOnlyList<LogVolumePointDto> VolumeOverTime
);

public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);