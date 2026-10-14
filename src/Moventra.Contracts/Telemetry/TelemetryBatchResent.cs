namespace Moventra.Contracts.Telemetry;

public sealed record TelemetryBatchResent
{
    // ID único do lote.
    public required Guid BatchId { get; init; }
    
    // ID do caminhão.
    public required string VehicleId { get; init; }
    
    // As leituras guardadas.
    public required IReadOnlyList<TelemetryRecorded> Readings { get; init; }
    
    // Quando o lote foi enviado
    public required DateTimeOffset SentAt { get; init; }
}