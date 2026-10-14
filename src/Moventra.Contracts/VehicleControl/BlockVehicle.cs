namespace Moventra.Contracts.VehicleControl;

public sealed record BlockVehicle
{
    // ID único do evento.
    public required Guid CommandId { get; init; }
    
    // ID do caminhão que deve ser bloqueado.
    public required string VehicleId { get; init; }
    
    // Motivo do bloqueio.
    public required string Reason { get; init; }
    
    // ID da infração que gerou o bloqueio.
    public Guid? ViolationId { get; init; }
    
    // Quando a central deu a ordem de bloqueio.
    public required DateTimeOffset IssuedAt { get; init; }
    
    // Até quando a ordem vale.
    public required DateTimeOffset ExpiresAt { get; init; }
}