// Domain/Entities/Incident.cs
namespace IncidentMonitor.Domain.Entities;

public class Incident
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string ServiceName { get; private set; } = default!;
    public string MessagePattern { get; private set; } = default!;
    public int OccurrenceCount { get; private set; }
    public DateTimeOffset FirstSeen { get; private set; }
    public DateTimeOffset LastSeen { get; private set; }
    public bool IsResolved { get; private set; }

    private readonly List<LogEntry> _logs = new();
    public IReadOnlyCollection<LogEntry> Logs => _logs.AsReadOnly();

    private Incident() { }

    public static Incident Open(string serviceName, string messagePattern, DateTimeOffset at) =>
        new()
        {
            ServiceName     = serviceName,
            MessagePattern  = messagePattern,
            OccurrenceCount = 1,
            FirstSeen       = at,
            LastSeen        = at
        };

    public void RecordOccurrence(DateTimeOffset at)
    {
        OccurrenceCount++;
        LastSeen = at;
    }

    public void Resolve() => IsResolved = true;
}