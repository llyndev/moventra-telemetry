using Moventra.Contracts.Telemetry;

namespace Moventra.Ingestor.Data;

public sealed class TelemetryReading
{

    public Guid ReadingId { get; set; }
    public string VehicleId { get; set; } = string.Empty;
    public long Sequence { get; set; }
    public DateTimeOffset RecordedAt { get; set; } // Quando foi feito a leitura 
    public DateTimeOffset ReceivedAt { get; set; } = DateTimeOffset.UtcNow;
    public double Latitude { get; set; }
    public double Longitude { get; set; }    
    public double SpeedKmh { get; set; }
    public double EngineTempCelsius { get; set; }
    public int Rpm { get; set; }

    public static TelemetryReading FromMessage(TelemetryRecorded message, DateTimeOffset receivedAt) =>
        new TelemetryReading()
        {
            ReadingId = message.ReadingId,
            VehicleId = message.VehicleId,
            Sequence = message.Sequence,
            RecordedAt = message.RecordedAt,
            Latitude = message.Latitude,
            Longitude = message.Longitude,
            SpeedKmh = message.SpeedKmh,
            Rpm = message.Rpm,
            EngineTempCelsius = message.EngineTempCelsius,
            ReceivedAt = receivedAt
        };
}