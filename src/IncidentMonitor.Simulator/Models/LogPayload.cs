namespace IncidentMonitor.Simulator.Models;

public sealed record LogPayload(
    string ServiceName,
    string Level,
    string Message,
    DateTimeOffset Timestamp
);

public sealed record LogBatch(IReadOnlyList<LogPayload> Logs);