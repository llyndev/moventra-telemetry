using MassTransit;
using Moventra.Contracts.Telemetry;
using Moventra.Contracts.VehicleControl;
using Moventra.RulesEngine.Rules;

namespace Moventra.RulesEngine.Consumers;

public sealed class SpeedRuleConsumer(ILogger<SpeedRuleConsumer> logger, SpeedingTracker tracker) : IConsumer<TelemetryRecorded>
{
    
    // Limite de velocidade.
    private const double SpeedLimitKmh = 90;

    public async Task Consume(ConsumeContext<TelemetryRecorded> context)
    {
        var violation = tracker.Check(context.Message);

        if (violation is null)
            return;

        await context.Publish(violation);

        var now = DateTimeOffset.UtcNow;

        var command = new BlockVehicle
        {
            CommandId = Guid.NewGuid(),
            VehicleId = violation.VehicleId,
            Reason = "Excesso de velocidade",
            ViolationId = violation.ViolationId,
            IssuedAt = now,
            ExpiresAt = now.AddMinutes(5)
        };

        var endpoint = await context.GetSendEndpoint(new Uri($"queue:vehicle-{violation.VehicleId}"));
        await endpoint.Send(command);
        
        logger.LogWarning("INFRAÇÃO: caminhão {VehicleId} acima de {Limit} km/h por 10s (máx. {Max} km/h).", violation.VehicleId, violation.SpeedLimitKmh, violation.MaxSpeedKmh);
    }
}