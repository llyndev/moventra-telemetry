namespace Moventra.Contracts.Rules;

public sealed record SpeedingViolationDetected
{
    // ID único da infração.
    public required Guid ViolationId { get; init; }
    
    // ID do caminhão que cometeu a infração.
    public required string VehicleId { get; init; }
    
    // Limite de velocidade que estava valendo.
    public required double SpeedLimitKmh { get; init; }
    
    // Maior velocidade registrada durante infração.
    public required double MaxSpeedKmh { get; init; }
    
    // Quando a velocidade passou do limite
    public required DateTimeOffset StartedAt { get; init; }
    
    // Quando o motor de regras confirmou a infração.
    public required DateTimeOffset DetectedAt { get; init; }
}