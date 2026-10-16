using Microsoft.EntityFrameworkCore;

namespace Moventra.Ingestor.Data;

public sealed class TelemetryDbContext(DbContextOptions<TelemetryDbContext> options) : DbContext(options)
{
    public DbSet<TelemetryReading> TelemetryReadings => Set<TelemetryReading>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TelemetryReading>(e =>
        {
            e.HasKey(x => new { x.ReadingId, x.RecordedAt });

            e.HasIndex(x => new { x.VehicleId, x.RecordedAt });
        });
    }
}