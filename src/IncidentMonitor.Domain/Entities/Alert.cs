// Domain/Entities/Alert.cs
namespace IncidentMonitor.Domain.Entities;

public class Alert
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string ServiceName { get; private set; } = default!;
    public string Description { get; private set; } = default!;
    public DateTimeOffset TriggeredAt { get; private set; }
    public bool IsAcknowledged { get; private set; }

    private Alert() { }

    public static Alert Trigger(string serviceName, string description) =>
        new()
        {
            ServiceName = serviceName,
            Description = description,
            TriggeredAt = DateTimeOffset.UtcNow
        };

    public void Acknowledge() => IsAcknowledged = true;
}