using System.Diagnostics.CodeAnalysis;

namespace Tellurian.Trains.Protocols.LocoNet.Programming;

public readonly struct CV
{
    public int Number
    {
        get;
        init => field = value is >= 1 and <= 1024
            ? value
            : throw new ArgumentOutOfRangeException(nameof(Number), "CV number must be 1-1024");
    }

    public byte Value { get; init; }

    public override bool Equals([NotNullWhen(true)] object? obj) => base.Equals(obj);
    public override int GetHashCode() => HashCode.Combine(Number, Value);
    public override string ToString() => $"CV{Number}={Value}";

    public static bool operator ==(CV left, CV right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(CV left, CV right)
    {
        return !(left == right);
    }
}
