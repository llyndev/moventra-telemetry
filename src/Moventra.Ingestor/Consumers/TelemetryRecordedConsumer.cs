using MassTransit;
using Moventra.Contracts.Telemetry;
using Moventra.Ingestor.Data;

namespace Moventra.Ingestor.Consumers;

public sealed class TelemetryRecordedConsumer(
    ILogger<TelemetryRecordedConsumer> logger,
    TelemetryDbContext db) : IConsumer<TelemetryRecorded>
{
    public async Task Consume(ConsumeContext<TelemetryRecorded> context)
    {
        var message = context.Message;

        var reading = new TelemetryReading
        {
            ReadingId = message.ReadingId,
            VehicleId = message.VehicleId,
            EngineTempCelsius = message.EngineTempCelsius,
            Latitude = message.Latitude,
            Longitude = message.Longitude,
            RecordedAt = message.RecordedAt,
            Rpm = message.Rpm,
            Sequence = message.Sequence,
            SpeedKmh = message.SpeedKmh
        };
        
        // "prepara" a linha para inserir.
        db.TelemetryReadings.Add(reading);
        
        // envia o INSERT ao banco
        // CancellationToken caso o Ingestor for desligado no meio.
        await db.SaveChangesAsync(context.CancellationToken);
        
        logger.LogInformation(
            "Leitura #{Sequence} do caminhão {VehicleId} salva.", reading.Sequence, reading.VehicleId);
    }
}