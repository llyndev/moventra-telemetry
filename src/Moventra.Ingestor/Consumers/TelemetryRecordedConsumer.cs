using MassTransit;
using Moventra.Contracts.Telemetry;

namespace Moventra.Ingestor.Consumers;

public sealed class TelemetryRecordedConsumer(ILogger<TelemetryRecordedConsumer> logger) : IConsumer<TelemetryRecorded>
{
    public Task Consume(ConsumeContext<TelemetryRecorded> context)
    {
        var reading = context.Message;
        
        logger.LogInformation(
            "Leitura #{Sequence} do caminhão {VehicleId}: {SpeedKmh} km/h", reading.Sequence, reading.VehicleId, reading.SpeedKmh);

        return Task.CompletedTask;
    }
}