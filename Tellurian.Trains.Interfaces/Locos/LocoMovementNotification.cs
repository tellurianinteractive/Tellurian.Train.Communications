using System.Runtime.Serialization;

namespace Tellurian.Trains.Interfaces.Locos;

[DataContract]
public sealed class LocoMovementNotification(Address address, Direction direction, Speed speed, DateTimeOffset timestamp) : LocoNotification(address, timestamp)
{
    [DataMember]
    private readonly Direction _Direction = direction;
    [DataMember]
    private readonly Speed _Speed = speed;

    public LocoMovementNotification(Address address, Direction direction, Speed speed) : this(address, direction, speed, DateTimeOffset.Now) { }

    public Direction Direction => _Direction;
    public Speed Speed => _Speed;
}
