namespace Moventra.Contracts.Telemetry;

// sealed = ninguém pode herdar nele (é so um formato de dados)
public sealed record TelemetryRecorded
{
    // required = obrigatório preencher ao criar.
    // init = só pode receber o valor na criação; depois não muda mais.
    
    // ID único de leitura, gerado no caminhão. Se reenviar a mesma leitura
    // Servidor descarta repetidos.
    public required Guid ReadingId { get; init; }
    
    // Código do caminhão, ex: "TKX-042".
    public required string VehicleId { get; init; }
    
    // Contator: 1, 2, 3... Define a ordem das leituras.
    public required long Sequence { get; init; }
    
    // Quando a leitura foi feita (relógio do caminhão, em UTC).
    public required DateTimeOffset RecordedAt { get; init; }
    
    // Dados dos sensores.
    public required double Latitude { get; init; }
    public required double Longitude { get; init; }
    public required double SpeedKmh { get; init; }
    public required int Rpm { get; init; }
    public required double EngineTempCelsius { get; init; }
}