namespace Moventra.Contracts.VehicleControl;

public sealed record BlockCommandRejected
{
    // ID do comando que foi recusado (O mesmo do BlockVehicle).
    public required Guid CommandId { get; init; }
    
    // ID do veiculo.
    public required string VehicleId { get; init; }
    
    // Motivo de não bloquear.
    public required string Reason { get; init; }
    
    // Velocidade do caminhão no momento da recusa de bloqueio.
    public required double SpeedKmh { get; init; }
    
    // Quando foi rejeitado.
    public required DateTimeOffset RejectedAt { get; init; }
}