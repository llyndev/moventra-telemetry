using MassTransit;
using Microsoft.EntityFrameworkCore;
using Moventra.Contracts.Telemetry;
using Moventra.Ingestor.Data;
using Npgsql;

namespace Moventra.Ingestor.Consumers;

public sealed class TelemetryRecordedConsumer(
    ILogger<TelemetryRecordedConsumer> logger,
    TelemetryDbContext db) : IConsumer<TelemetryRecorded>
{
    public async Task Consume(ConsumeContext<TelemetryRecorded> context)
    {
        var message = context.Message;

        var reading = TelemetryReading.FromMessage(message, DateTimeOffset.UtcNow);
        
        // "prepara" a linha para inserir.
        db.TelemetryReadings.Add(reading);

        try
        {
            // envia o INSERT ao banco
            // CancellationToken caso o Ingestor for desligado no meio.
            await db.SaveChangesAsync(context.CancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException
                                           {
                                               SqlState: PostgresErrorCodes.UniqueViolation
                                           })
        {
            logger.LogInformation("Leitura {reading.ReadingId} foi ignorada por ser duplicada.", reading.ReadingId);
            return;
        }
        
        
        logger.LogInformation(
            "Leitura #{Sequence} do caminhão {VehicleId} salva.", reading.Sequence, reading.VehicleId);
    }
}