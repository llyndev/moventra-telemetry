namespace Moventra.Contracts.VehicleControl;

public sealed record BlockCommandReceived
{
    // ID do comando que foi recusado (O mesmo do BlockVehicle).
    public required Guid CommandId { get; init; }
    
    // ID do veiculo.
    public required string VehicleId {get; init; }
    
    // Quando foi recebido o evento
    public required DateTimeOffset ReceivedAt { get; init; }
}