using System.Runtime.Serialization;

namespace Tellurian.Trains.Interfaces.Locos;

[DataContract]
public readonly struct LocoAddress : IEquatable<LocoAddress>
{
    /// <summary>
    /// Static method for creating a <see cref="LocoAddress"/> from a number.
    /// </summary>
    public static LocoAddress From(int shortOrLongAddress) => new((short)shortOrLongAddress);
    /// <summary>
    /// Creates a LocoAddress from LocoNet-encoded bytes.
    /// </summary>
    /// <param name="high">High 7 bits of the address (0 for short addresses 1-127).</param>
    /// <param name="low">Low 7 bits of the address.</param>
    /// <returns>A LocoAddress reconstructed from the LocoNet encoding.</returns>
    public static LocoAddress From(byte high, byte low)
    {
        return From((high << 7) | low);
    }
    /// <summary>
    /// Creates a new instance of <see cref="LocoAddress"/> from a two-byte buffer.
    /// </summary>
    /// <param name="buffer">A byte array containing exactly two bytes representing the address to convert.
    /// The first byte is high address byte and the seconc is low address byte.</param>
    /// <returns>A <see cref="LocoAddress"/> instance constructed from the specified buffer.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="buffer"/> does not contain exactly two bytes.</exception>
    public static LocoAddress From(byte[] buffer) 
    {
        ArgumentNullException.ThrowIfNull(buffer);
        if (buffer.Length != 2) throw new ArgumentOutOfRangeException(nameof(buffer), "Buffer must contain  2 bytes.");
        return From(buffer[0], buffer[1]);
    }
    /// <summary>
    /// Represents a zero loco address (used as sentinel for "no address").
    /// </summary>
    public static LocoAddress Zero => new(0);
    /// <summary>
    /// Constructs a <see cref="LocoAddress"/> from a number.
    /// </summary>
    /// <param name="number">The loco address (1-9999).</param>
    private LocoAddress(short number)
    {
        Number = number;
    }

    /// <summary>
    /// High 7 bits of the address for LocoNet encoding.
    /// Returns 0 for short addresses (1-127).
    /// </summary>
    public byte High => (byte)( IsShort ? 0 : (Number >> 7));

    /// <summary>
    /// Low 7 bits of the address for LocoNet encoding.
    /// </summary>
    public byte Low => (byte)(Number & 0x7F);

    /// <summary>
    /// Tests if a locomotive address is valid or not. A valid address should be in the range 0-9999.
    /// Zero is allowed as a sentinel value for "no address".
    /// </summary>
    /// <param name="number">The address value to verify.</param>
    /// <returns>True if valid; otherwise false.</returns>
    public static bool IsValid(short number) => number >= 0 && number <= 9999;

    /// <summary>
    /// The address.
    /// </summary>
    [field: DataMember(Name = "number")]
    public short Number
    {
        get;
        init => field = IsValid(value) ? value : throw new ArgumentOutOfRangeException(nameof(value), "Address must be 0-9999.");
    }

    /// <summary>
    /// The loco address is 128 or above.
    /// </summary>
    public bool IsLong => Number >= 128;

    /// <summary>
    /// The loco address is max 127.
    /// </summary>
    public bool IsShort => Number < 128;

    /// <summary>
    /// The addresses 100 to 127 can sometimes cause trouble because some systems regards 1-99 as short and some 1-127.
    /// </summary>
    public bool IsShortTwoDigit => IsShort && Number < 100;

    /// <summary>
    /// The addresses 100 to 127 can sometimes cause trouble because some systems regards 1-99 as short and some 1-127.
    /// </summary>
    public bool IsShortThreeDigit => IsShort && Number >= 100;

    public bool Equals(LocoAddress other) => other.Number == Number;
    public override bool Equals(object? obj) => obj is LocoAddress other && Equals(other);
    public override int GetHashCode() => Number.GetHashCode();
    public override string ToString() => $"{Number}";
    public static bool operator ==(LocoAddress left, LocoAddress right) => left.Equals(right);
    public static bool operator !=(LocoAddress left, LocoAddress right) => !(left == right);
}
