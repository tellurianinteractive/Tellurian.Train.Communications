#pragma warning disable CA1303 // Do not pass literals as localized parameters

namespace Tellurian.Trains.Protocols.XpressNet;

public struct AccessoryAddress : IEquatable<AccessoryAddress>
{
    private readonly short Number;

    public AccessoryAddress(short number)
    {
        if (!IsValid(number)) throw new ArgumentOutOfRangeException(nameof(number), "Address should be 1-1024.");
        Number = number;
    }

    public static bool IsValid(short number) => number >= 1 && number <= 1024;

    public byte Group => (byte)((Number - 1) / 4);
    public byte Subaddress => (byte)(Number % 4);

    public byte[] GetBytes()
    {
        var result = new byte[2];
        var a = BitConverter.GetBytes(Number - 1);
        result[0] = a[1];
        result[1] = a[0];
        return result;
    }

    public bool Equals(AccessoryAddress other) => other.Number == Number;
    public override bool Equals(object? obj) => obj is AccessoryAddress other && Equals(other);
    public override int GetHashCode() => Number.GetHashCode();

    public static bool operator ==(AccessoryAddress left, AccessoryAddress right) => left.Equals(right);
    public static bool operator !=(AccessoryAddress left, AccessoryAddress right) => !(left == right);
}
