using IncidentMonitor.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IncidentMonitor.Infrastructure.Persistence.Configurations;

public sealed class AlertConfiguration : IEntityTypeConfiguration<Alert>
{
    public void Configure(EntityTypeBuilder<Alert> builder)
    {
        builder.ToTable("alerts");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.ServiceName)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(a => a.Description)
               .IsRequired()
               .HasMaxLength(1000);

        builder.Property(a => a.TriggeredAt).IsRequired();
        builder.Property(a => a.IsAcknowledged).IsRequired();

        builder.HasIndex(a => a.IsAcknowledged);
        builder.HasIndex(a => a.TriggeredAt);
    }
}