using System.Globalization;

namespace Tellurian.Trains.Protocols.XpressNet;

public readonly struct LocoAddress : IEquatable<LocoAddress> {
    public static LocoAddress From(short number) => new (number);

    public LocoAddress(short number) {
        if (! IsValid(number)) throw new ArgumentOutOfRangeException(nameof(number));
        Number = number;
    }

    public LocoAddress(byte[] data) {
        var span = data.AsSpan();
        span.Reverse();
        var buffer = span.ToArray();
        buffer[1] &= 0x3F;
        Number = BitConverter.ToInt16(buffer,0);
    }

    public static bool IsValid(short number) => number >= 1 && number <= 9999;

    public bool IsLong => Number >= 128;
    public bool IsShort => Number < 128;
    /// <summary>
    /// The adresses 100 to 127 can sometimes cause trouble because some systems regards 1-99 as short and some 1-127.
    /// </summary>
    public bool IsShortThreeDigit => IsShort && (Number >= 100);
    public short Number { get; }
    /// <summary>
    /// Get two byte loco address accoroding XpressNet specifictaion.
    /// </summary>
    /// <remarks>
    /// Z21 ignores the two high bits required for long adresses.
    /// </remarks>
    public byte[] GetBytesAccordingToXpressNet() {
        var result = new byte[2];
        var a = BitConverter.GetBytes(Number);
        result[0] = a[1];
        if (IsLong) result[0] += 192;
        result[1] = a[0];
        return result;
    }

    public override string ToString() => string.Format(CultureInfo.InvariantCulture, "{0}", Number);
    public bool Equals(LocoAddress other) => Number == other.Number;
    public override bool Equals(object? obj) => obj is LocoAddress other && other.Equals(this);
    public override int GetHashCode() => Number.GetHashCode();
    public static bool operator ==(LocoAddress left, LocoAddress right) => left.Equals(right);
    public static bool operator !=(LocoAddress left, LocoAddress right) => !(left == right);
}


public static class LocoAdressExtensions
{
    extension (LocoAddress locoAddress)
    {

    }
    public static Interfaces.Locos.LocoAddress Map(this LocoAddress me) => Interfaces.Locos.LocoAddress.From(me.Number);
    public static LocoAddress Map(this Interfaces.Locos.LocoAddress me) => LocoAddress.From(me.Number);
}
