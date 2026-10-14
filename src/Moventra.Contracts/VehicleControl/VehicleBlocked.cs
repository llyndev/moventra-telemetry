namespace Moventra.Contracts.VehicleControl;

public record VehicleBlocked
{
    // ID do comando que foi recusado (O mesmo do BlockVehicle).
    public required Guid CommandId { get; init; }
    
    // ID do veiculo.
    public required string VehicleId { get; init; }
    
    // Quando foi bloqueado.
    public required DateTimeOffset BlockedAt { get; init; }
}