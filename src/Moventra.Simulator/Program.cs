using MassTransit;
using Moventra.Contracts.Telemetry;
using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddUserSecrets<Program>()
    .Build();

var rabbitHost = config["RabbitMq:Host"]
                 ?? throw new InvalidOperationException("RabbitMq:Host não configurado.");

var rabbitUser = config["RabbitMq:Username"]
                 ?? throw new InvalidOperationException("RabbitMq:Username não configurado.");

var rabbitPassword = config["RabbitMq:Password"]
                 ?? throw new InvalidOperationException("RabbitMq:Password não configurado.");

// Código do caminhão sendo simulado.
var vehicleId = config["Simulator:VehicleId"]
                ?? throw new InvalidOperationException("Simulator:VehicleId não encontrado.");

// Gerador de números aleatórios para simular os sensores.
var random = new Random();

// Contador de leituras
long sequence = 0;

// Posição inicial do caminhão.
double latitude = -23.5505;
double longitude = -46.6333;

double speedKmh = 80;

// CancellationTokenSource = "botão de parar".
using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true; // não encerra o programa de imediato
    cts.Cancel();
};

var bus = Bus.Factory.CreateUsingRabbitMq(cfg =>
{
    cfg.Host(rabbitHost, "/", h =>
    {
        h.Username(rabbitUser);
        h.Password(rabbitPassword);
    });
});

await bus.StartAsync(cts.Token);

Console.WriteLine($"Caminhão {vehicleId} ligado. Ctrl+C para desligar.");

while (!cts.Token.IsCancellationRequested)
{
    sequence++;
    speedKmh += random.Next(-5, 6);
    speedKmh = Math.Clamp(speedKmh, 40, 120);

    var reading = new TelemetryRecorded
    {
        ReadingId = Guid.NewGuid(),
        VehicleId = vehicleId,
        Sequence = sequence,
        RecordedAt = DateTimeOffset.UtcNow,
        Latitude = latitude,
        Longitude = longitude,
        SpeedKmh = speedKmh,
        Rpm = random.Next(1200, 2201),
        EngineTempCelsius = random.Next(85, 96)
    };
    
    Console.WriteLine($"#{reading.Sequence} | {reading.SpeedKmh} km/h | {reading.Rpm} rpm |  {reading.EngineTempCelsius} °C");

    await bus.Publish(reading);
    
    latitude += 0.0001;

    try
    {
        await Task.Delay(TimeSpan.FromSeconds(1), cts.Token);
    }
    catch (OperationCanceledException)
    {
        break;
    }
}

await bus.StopAsync();

Console.WriteLine("Caminhão desligado.");


