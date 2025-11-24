namespace Tellurian.Trains.Protocols.LocoNet;

public readonly struct AccessoryAddress(ushort address, AccessoryInput input) : IEquatable<AccessoryAddress>
{
    public ushort Value { get; } = address;
    public AccessoryInput Input { get; } = input;
    public bool Equals(AccessoryAddress other) => other.Value == Value && other.Input == Input;
    public override bool Equals(object? obj) => obj is AccessoryAddress other && Equals(other);
    public override int GetHashCode() => (Value.GetHashCode() / 2) + (Input.GetHashCode() / 2);
    public override string ToString() => $"{Value}:{Input}";
    public static bool operator ==(AccessoryAddress left, AccessoryAddress right) => left.Equals(right);
    public static bool operator !=(AccessoryAddress left, AccessoryAddress right) => !(left == right);
}

public enum AccessoryInput : byte
{
    Port0 = 0b00,
    Port1 = 0b01,
    Port2 = 0b10,
    Port3 = 0b11
}

public enum AccessoryFunction : byte
{
    ClosedOrGreen = 0b0,
    ThrownOrRed = 0b1
}

public enum OutputState : byte
{
    Off = 0b0,
    On = 0b1
}
