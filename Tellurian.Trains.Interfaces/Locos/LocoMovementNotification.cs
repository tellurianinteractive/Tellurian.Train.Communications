using System.Text.Json.Serialization;

namespace Tellurian.Trains.Interfaces.Locos;

public sealed class LocoMovementNotification(Address address, Direction direction, Speed speed, DateTimeOffset timestamp) : LocoNotification(address, timestamp)
{
    [JsonConstructor]
    public LocoMovementNotification(Address address, Direction direction, Speed speed) : this(address, direction, speed, DateTimeOffset.Now) { }

    public Direction Direction { get; } = direction;
    public Speed Speed { get; } = speed;
}
