using IncidentMonitor.Domain.Enums;

namespace IncidentMonitor.Domain.Entities;

public class LogEntry
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string ServiceName { get; private set; } = default!;
    public LogLevel Level { get; private set; }
    public string Message { get; private set; } = default!;
    public DateTimeOffset Timestamp { get; private set; }
    public Guid? IncidentId { get; private set; }

    private LogEntry() { }

    public static LogEntry Create(string serviceName, LogLevel level,
        string message, DateTimeOffset timestamp)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceName);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        return new LogEntry
        {
            ServiceName = serviceName,
            Level       = level,
            Message     = message,
            Timestamp   = timestamp
        };
    }

    public void AssignToIncident(Guid incidentId) => IncidentId = incidentId;
}