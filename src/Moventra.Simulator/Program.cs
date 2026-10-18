using MassTransit;
using Moventra.Contracts.Telemetry;
using Microsoft.Extensions.Configuration;
using Moventra.Contracts.VehicleControl;

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

double speedKmh = 85;

BlockVehicle? pendingBlock = null;

// O caminhão já esta bloqueado?
var isBlocked = false;

var commandLock = new Lock();

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
    
    cfg.ReceiveEndpoint($"vehicle-{vehicleId}", e =>
    {
        e.Handler<BlockVehicle>(async ctx =>
        {
            var command = ctx.Message;
            var now = DateTimeOffset.UtcNow;

            await ctx.Publish(new BlockCommandReceived
            {
                CommandId = command.CommandId,
                VehicleId = vehicleId,
                ReceivedAt = now
            });

            if (now > command.ExpiresAt)
            {
                await ctx.Publish(new BlockCommandRejected
                {
                    CommandId = command.CommandId,
                    VehicleId = vehicleId,
                    Reason = "Comando expirado",
                    SpeedKmh = speedKmh,
                    RejectedAt = now
                });
                Console.WriteLine(">>> Comando EXPIRADO. Recusado.");
                return;
            }

            lock (commandLock)
            {
                pendingBlock = command;
            }
            Console.WriteLine($">>> COMANDO DE BLOQUEIO recebido! Motivo: {ctx.Message.Reason}");
        });
    });
});

await bus.StartAsync(cts.Token);

Console.WriteLine($"Caminhão {vehicleId} ligado. Ctrl+C para desligar.");

// sinal do caminhão
var hasSignal = true;

// leituras guardadas enquanto estava sem sinal
var buffer = new List<TelemetryRecorded>();

while (!cts.Token.IsCancellationRequested)
{

    BlockVehicle? block;
    lock (commandLock)
    {
        block = pendingBlock;
    }

    if (isBlocked)
    {
        // Bloqueado: não anda.
        speedKmh = 0;
    } else if (block is not null)
    {
        speedKmh = Math.Max(speedKmh - 5, 0);
        
        if (speedKmh == 0)
        {
            var vehicleBlocked = new VehicleBlocked
            {
                CommandId = block.CommandId,
                VehicleId = vehicleId,
                BlockedAt = DateTimeOffset.UtcNow
            };

            await bus.Publish(vehicleBlocked);
            
            isBlocked = true;
            Console.WriteLine(">>> VEÍCULO BLOQUEADO.");
        }
    }
    else
    {
        speedKmh += random.Next(-5, 6);
        speedKmh = Math.Clamp(speedKmh, 90, 120);
    }
    
    sequence++;

    if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.S)
    {
        hasSignal = !hasSignal;
        Console.WriteLine(hasSignal ? ">>> SINAL VOLTOU" : ">>> SEM SINAL");
    }

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

    if (hasSignal)
    {
        // sinal voltou e tem leituras guardadas, reenvia antes da leitura atual
        if (buffer.Count > 0)
        {
            // divide a lista em pedaços de no máximo 100.
            foreach (var chunk in buffer.Chunk(100))
            {
                await bus.Publish(new TelemetryBatchResent
                {
                    BatchId = Guid.NewGuid(),
                    VehicleId = vehicleId,
                    Readings = chunk,
                    SentAt = DateTimeOffset.UtcNow
                });
            }
            
            Console.WriteLine($">>> Reenviadas {buffer.Count} leituras guardadas.");
            buffer.Clear();
        }
        await bus.Publish(reading);
    }
    else
    {
        buffer.Add(reading);
        Console.WriteLine($">>> Leitura guardada, quantidade de leituras: {buffer.Count}");
    }
    
    
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


