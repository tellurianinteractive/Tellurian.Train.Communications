namespace Tellurian.Trains.Interfaces.Locos;

public sealed class LocoMovementNotification(Address address, Direction direction, Speed speed, DateTimeOffset timestamp) : LocoNotification(address, timestamp)
{
    private readonly Direction _Direction = direction;
    private readonly Speed _Speed = speed;

    public LocoMovementNotification(Address address, Direction direction, Speed speed) : this(address, direction, speed, DateTimeOffset.Now) { }

    public Direction Direction => _Direction;
    public Speed Speed => _Speed;
}
