using System.Runtime.Serialization;
namespace Tellurian.Trains.Interfaces.Locos;

[DataContract]
public readonly struct LocoDrive : IEquatable<LocoDrive>
{
    [DataMember]
    public LocoDirection Direction { get; init; }
    [DataMember]
    public LocoSpeed Speed { get; init; }

    public readonly bool Equals(LocoDrive other) => other.Direction == Direction && other.Speed == Speed;
    public override bool Equals(object? obj) => obj is LocoDrive other && Equals(other);
    public override int GetHashCode() => (Direction.GetHashCode() / 2) + (Speed.GetHashCode() / 2);
    public override string ToString() => $"{Speed} {Direction}";
    public static bool operator ==(LocoDrive left, LocoDrive right) => left.Equals(right);
    public static bool operator !=(LocoDrive left, LocoDrive right) => !(left == right);
}
