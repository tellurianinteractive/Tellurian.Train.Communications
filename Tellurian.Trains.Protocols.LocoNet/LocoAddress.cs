namespace Tellurian.Trains.Protocols.LocoNet;

public readonly struct LocoAddress : IEquatable<LocoAddress>
{
    public LocoAddress(ushort address)
    {
        if (address < 1 || address > 9999) throw new ArgumentOutOfRangeException(nameof(address));
        Address = address;
    }
    public ushort Address { get; }
    public byte High => (byte)(IsShort ? 0 : (Address >> 7));
    public byte Low => (byte)(Address & 0x7F);
    public bool IsLong => Address >= 128;
    public bool IsShort => Address < 128;
    public bool Equals(LocoAddress other) => other.Address == Address;
    public override bool Equals(object? obj)=> obj is LocoAddress other && Equals(other);
    public override int GetHashCode() => Address.GetHashCode();
    public static bool operator ==(LocoAddress left, LocoAddress right) => left.Equals(right);
    public static bool operator !=(LocoAddress left, LocoAddress right) => !(left == right);
}