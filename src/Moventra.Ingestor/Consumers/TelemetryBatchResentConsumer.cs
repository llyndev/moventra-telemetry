using MassTransit;
using Microsoft.EntityFrameworkCore;
using Moventra.Contracts.Telemetry;
using Moventra.Ingestor.Data;

namespace Moventra.Ingestor.Consumers;

public sealed class TelemetryBatchResentConsumer(ILogger<TelemetryBatchResentConsumer> logger, TelemetryDbContext db)
    : IConsumer<TelemetryBatchResent>
{
    public async Task Consume(ConsumeContext<TelemetryBatchResent> context)
    {
        var batch = context.Message;
        var receivedAt = DateTimeOffset.UtcNow;

        // Os IDs de todas as leituras do lote.
        var ids = batch.Readings.Select(r => r.ReadingId).ToList();

        var existing = await db.TelemetryReadings
            .Where(r => ids.Contains(r.ReadingId))
            .Select(r => r.ReadingId)
            .ToListAsync(context.CancellationToken);

        var newReadings = batch.Readings
            .Where(r => !existing.Contains(r.ReadingId))
            .Select(r => TelemetryReading.FromMessage(r, receivedAt))
            .ToList();

        // add várias de uma vez.
        db.TelemetryReadings.AddRange(newReadings);
        await db.SaveChangesAsync(context.CancellationToken);
        
        logger.LogInformation(
            "Lote {BatchId}: {New} leituras novas, {Existing} já eixstiam.", batch.BatchId, newReadings.Count, existing.Count);

    }
}