using IncidentMonitor.Domain.Entities;
using IncidentMonitor.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IncidentMonitor.Infrastructure.Persistence.Configurations;

public sealed class LogEntryConfiguration : IEntityTypeConfiguration<LogEntry>
{
    public void Configure(EntityTypeBuilder<LogEntry> builder)
    {
        builder.ToTable("logs");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.ServiceName)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(l => l.Level)
               .IsRequired()
               .HasConversion<string>()   // stores "Info" / "Warn" / "Error"
               .HasMaxLength(10);

        builder.Property(l => l.Message)
               .IsRequired()
               .HasMaxLength(2000);

        builder.Property(l => l.Timestamp)
               .IsRequired();

        builder.Property(l => l.IncidentId)
               .IsRequired(false);

        // Query patterns we know we'll hit
        builder.HasIndex(l => l.ServiceName);
        builder.HasIndex(l => l.Timestamp);
        builder.HasIndex(l => new { l.ServiceName, l.Level, l.Timestamp });
    }
}