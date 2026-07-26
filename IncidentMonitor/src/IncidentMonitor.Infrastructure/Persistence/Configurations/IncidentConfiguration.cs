using IncidentMonitor.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IncidentMonitor.Infrastructure.Persistence.Configurations;

public sealed class IncidentConfiguration : IEntityTypeConfiguration<Incident>
{
    public void Configure(EntityTypeBuilder<Incident> builder)
    {
        builder.ToTable("incidents");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.ServiceName)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(i => i.MessagePattern)
               .IsRequired()
               .HasMaxLength(500);

        builder.Property(i => i.OccurrenceCount).IsRequired();
        builder.Property(i => i.FirstSeen).IsRequired();
        builder.Property(i => i.LastSeen).IsRequired();
        builder.Property(i => i.IsResolved).IsRequired();

        builder.HasIndex(i => new { i.ServiceName, i.IsResolved });
    }
}