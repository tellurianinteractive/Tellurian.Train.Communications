namespace Tellurian.Trains.Communications.Interfaces.Locos;

public readonly struct Drive : IEquatable<Drive>
{
    public Direction Direction { get; init; }
    public Speed Speed { get; init; }

    public readonly bool Equals(Drive other) => other.Direction == Direction && other.Speed == Speed;
    public override bool Equals(object? obj) => obj is Drive other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Direction, Speed);
    public override string ToString() => $"{Speed} {Direction}";
    public static bool operator ==(Drive left, Drive right) => left.Equals(right);
    public static bool operator !=(Drive left, Drive right) => !(left == right);
}
