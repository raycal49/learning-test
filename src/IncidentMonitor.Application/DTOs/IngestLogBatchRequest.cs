namespace IncidentMonitor.Application.DTOs;

public sealed record IngestLogRequest(
    string ServiceName,
    string Level,
    string Message,
    DateTimeOffset? Timestamp
);

public sealed record IngestLogBatchRequest(
    IReadOnlyList<IngestLogRequest> Logs
);