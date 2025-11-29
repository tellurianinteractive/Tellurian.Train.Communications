using System.Runtime.Serialization;

namespace Tellurian.Trains.Interfaces.Locos;

[DataContract]
public readonly struct Function : IEquatable<Function>
{
    public static Function On(Functions number) => new(number, true);
    public static Function Off(Functions number) => new(number, false);
    public static Function Set(Functions number, bool isOn) => new(number, isOn);

    private Function(Functions number, bool isOn)
    {
        _number = (byte)number;
        _isOn = isOn;
    }

    [DataMember(Name = "Number", Order = 1)]
    private readonly byte _number;

    [DataMember(Name = "On", Order = 2)]
    private readonly bool _isOn;

    public Functions Number => (Functions)_number;
    public bool IsOn => _isOn;
    public bool IsOff => !IsOn;
    public bool Equals(Function other) => other.Number == Number && other.IsOn == IsOn;
    public override bool Equals(object? obj) => obj is Function other && Equals(other);
    public override int GetHashCode() => (Number.GetHashCode() / 2) + (IsOn.GetHashCode() / 2);
    public override string ToString() => $"F{Number} {Status}";
    private string Status => IsOn ? "on" : "off";
    public static bool operator ==(Function left, Function right) => left.Equals(right);
    public static bool operator !=(Function left, Function right) => !(left == right);
}
