using System.Runtime.Serialization;

namespace Tellurian.Trains.Interfaces.Locos;

[DataContract]
public sealed class MovementLocoNotification(LocoAddress address, LocoDirection direction, LocoSpeed speed, DateTimeOffset timestamp) : LocoNotification(address, timestamp)
{
    [DataMember]
    private readonly LocoDirection _Direction = direction;
    [DataMember]
    private readonly LocoSpeed _Speed = speed;

    public MovementLocoNotification(LocoAddress address, LocoDirection direction, LocoSpeed speed) : this(address, direction, speed, DateTimeOffset.Now) { }

    public LocoDirection Direction => _Direction;
    public LocoSpeed Speed => _Speed;
}
