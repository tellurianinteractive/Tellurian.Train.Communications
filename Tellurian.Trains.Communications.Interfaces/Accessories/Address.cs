namespace Tellurian.Trains.Communications.Interfaces.Accessories;

/// <summary>
/// Represents a protocol-agnostic accessory (switch/turnout) address.
/// </summary>
public readonly struct Address : IEquatable<Address>
{
    /// <summary>
    /// Static method for creating an <see cref="Address"/> from a number.
    /// </summary>
    public static Address From(short accessoryAddress) =>
        new(accessoryAddress);
    /// <summary>
    /// Creates an <see cref="Address"/> from low and high bits.
    /// </summary>
    /// <param name="lowBits"></param>
    /// <param name="highBits"></param>
    /// <returns></returns>
    public static Address From(byte lowBits, byte highBits) =>
        From((short)(lowBits | ((highBits & 0x0F) << 7)));
    /// <summary>
    /// Constructs an <see cref="Address"/> from a number.
    /// </summary>
    /// <param name="number">The accessory address (1-2048).</param>
    private Address(short number)
    {
        Number = number;
    }

    /// <summary>
    /// Tests if an accessory address is valid. Valid range is 1-2048.
    /// User addresses are 1-based; protocols convert to 0-based wire addresses internally.
    /// </summary>
    /// <param name="number">The address value to verify.</param>
    /// <returns>True if valid; otherwise false.</returns>
    public static bool IsValid(short number) => number >= 1 && number <= 2048;

    /// <summary>
    /// The accessory address value (1-2048).
    /// </summary>
    public short Number
    {
        get;
        init => field = IsValid(value) ? value : throw new ArgumentOutOfRangeException(nameof(value), "Address must be 1-2048.");
    }

    /// <summary>
    /// The 0-based wire address for protocol encoding (Number - 1).
    /// Use this when encoding addresses for LocoNet, XpressNet, or other DCC protocols.
    /// </summary>
    public short WireAddress => (short)(Number - 1);

    /// <summary>
    /// Creates an <see cref="Address"/> from a 0-based wire address.
    /// Use this when decoding addresses from LocoNet, XpressNet, or other DCC protocols.
    /// </summary>
    /// <param name="wireAddress">The 0-based wire address (0-2047).</param>
    /// <returns>An Address with the corresponding 1-based user address.</returns>
    public static Address FromWireAddress(short wireAddress) =>
        From((short)(wireAddress + 1));

    public bool Equals(Address other) => other.Number == Number;
    public override bool Equals(object? obj) => obj is Address other && Equals(other);
    public override int GetHashCode() => Number.GetHashCode();
    public override string ToString() => $"{Number}";
    public static bool operator ==(Address left, Address right) => left.Equals(right);
    public static bool operator !=(Address left, Address right) => !(left == right);
}
