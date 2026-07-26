using IncidentMonitor.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IncidentMonitor.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<LogEntry>  Logs      => Set<LogEntry>();
    public DbSet<Incident>  Incidents => Set<Incident>();
    public DbSet<Alert>     Alerts    => Set<Alert>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Map all column names to snake_case (PostgreSQL convention)
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            entity.SetTableName(ToSnakeCase(entity.GetTableName()!));

            foreach (var property in entity.GetProperties())
                property.SetColumnName(ToSnakeCase(property.GetColumnName()!));

            foreach (var key in entity.GetKeys())
                key.SetName(ToSnakeCase(key.GetName()!));

            foreach (var index in entity.GetIndexes())
                index.SetDatabaseName(ToSnakeCase(index.GetDatabaseName()!));
        }

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    private static string ToSnakeCase(string name) =>
        string.Concat(name.Select((c, i) =>
            i > 0 && char.IsUpper(c) ? "_" + c : c.ToString()))
        .ToLowerInvariant();
}