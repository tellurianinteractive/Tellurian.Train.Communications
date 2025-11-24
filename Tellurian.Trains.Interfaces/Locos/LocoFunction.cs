using System.Runtime.Serialization;

namespace Tellurian.Trains.Interfaces.Locos;

[DataContract]
public readonly struct LocoFunction : IEquatable<LocoFunction>
{
    public static LocoFunction On(LocoFunctionNumber number) => new ( number, true);
    public static LocoFunction Off(LocoFunctionNumber number) => new ( number, false);
    public static LocoFunction Set(LocoFunctionNumber number, bool isOn) => new ( number, isOn);

    private LocoFunction(LocoFunctionNumber number, bool isOn)
    {
        _number = (byte)number;
        _isOn = isOn;
    }

    [DataMember(Name = "Number", Order = 1)]
    private readonly byte _number;

    [DataMember(Name = "On", Order = 2)]
    private readonly bool _isOn;

    public LocoFunctionNumber Number => (LocoFunctionNumber)_number;
    public bool IsOn => _isOn;
    public bool IsOff => !IsOn;
    public bool Equals(LocoFunction other) => other.Number == Number && other.IsOn == IsOn;
    public override bool Equals(object? obj) => obj is LocoFunction other && Equals(other);
    public override int GetHashCode() => (Number.GetHashCode() / 2) + (IsOn.GetHashCode() / 2);
    public override string ToString() => $"F{Number} {Status}";
    private string Status => IsOn ? "on" : "off";
    public static bool operator ==(LocoFunction left, LocoFunction right) => left.Equals(right);
    public static bool operator !=(LocoFunction left, LocoFunction right) => !(left == right);
}

public static class LocoFunctionExtensions
{
    public static LocoFunction[] Map(this (int F, bool on)[] me)
    {
        return me.Select(m => LocoFunction.Set((LocoFunctionNumber)m.F, m.on)).ToArray();
    }
}

[DataContract]
#pragma warning disable CA1028 // Enum Storage should be Int32
public enum LocoFunctionNumber : byte
{
    F0,
    F1,
    F2,
    F3,
    F4,
    F5,
    F6,
    F7,
    F8,
    F9,
    F10,
    F11,
    F12,
    F13,
    F14,
    F15,
    F16,
    F17,
    F18,
    F19,
    F20,
    F21,
    F22,
    F23,
    F24,
    F25,
    F26,
    F27,
    F28
}