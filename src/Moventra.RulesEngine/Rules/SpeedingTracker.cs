using Moventra.Contracts.Rules;
using Moventra.Contracts.Telemetry;

namespace Moventra.RulesEngine.Rules;

// A "memória" guarda, para cada caminhão, desde que ele está acima do limite.
public sealed class SpeedingTracker
{
    private const double SpeedLimitKmh = 90;
    private static readonly TimeSpan MinDuration = TimeSpan.FromSeconds(10);

    private sealed class State
    {
        public DateTimeOffset StartedAt { get; init; }
        public double MaxSpeedKmh { get; set; }
        public bool Alerted { get; set; }
    }

    // Dictionary = VehicleId -> State.
    private readonly Dictionary<string, State> _states = new();

    // o Lock garante que so mexe uma vez no _states
    private readonly Lock _lock = new();

    public SpeedingViolationDetected? Check(TelemetryRecorded reading)
    {
        lock (_lock)
        {
            if (reading.SpeedKmh <= SpeedLimitKmh)
            {
                // Remove o estado do veiculo, quando volta ao limite permitido.
                _states.Remove(reading.VehicleId);
                return null;
            }

            if (!_states.TryGetValue(reading.VehicleId, out var state))
            {
                state = new State { StartedAt = reading.RecordedAt, MaxSpeedKmh = reading.SpeedKmh };
                _states[reading.VehicleId] = state;
            }

            // Guarda a maior velocidade da infração.
            state.MaxSpeedKmh = Math.Max(state.MaxSpeedKmh, reading.SpeedKmh);

            // Tempo acima do limite.
            var duration = reading.RecordedAt - state.StartedAt;

            if (duration < MinDuration || state.Alerted)
                return null;

            state.Alerted = true;

            return new SpeedingViolationDetected
            {
                ViolationId = Guid.NewGuid(),
                VehicleId = reading.VehicleId,
                SpeedLimitKmh = SpeedLimitKmh,
                MaxSpeedKmh = state.MaxSpeedKmh,
                StartedAt = state.StartedAt,
                DetectedAt = DateTimeOffset.UtcNow
            };
        }
    }

}