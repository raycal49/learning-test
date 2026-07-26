namespace IncidentMonitor.Infrastructure.Messaging;

public sealed class RabbitMqSettings
{
    public const string SectionName = "RabbitMQ";

    public string Host  { get; set; } = "localhost";
    public string User  { get; set; } = "guest";
    public string Pass  { get; set; } = "guest";
    public int    Port  { get; set; } = 5672;
    public string VHost { get; set; } = "/";
}