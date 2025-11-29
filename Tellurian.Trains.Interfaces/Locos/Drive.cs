using System.Runtime.Serialization;
namespace Tellurian.Trains.Interfaces.Locos;

[DataContract]
public readonly struct Drive : IEquatable<Drive>
{
    [DataMember]
    public Direction Direction { get; init; }
    [DataMember]
    public Speed Speed { get; init; }

    public readonly bool Equals(Drive other) => other.Direction == Direction && other.Speed == Speed;
    public override bool Equals(object? obj) => obj is Drive other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Direction, Speed);
    public override string ToString() => $"{Speed} {Direction}";
    public static bool operator ==(Drive left, Drive right) => left.Equals(right);
    public static bool operator !=(Drive left, Drive right) => !(left == right);
}
