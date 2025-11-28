using System.Runtime.Serialization;

namespace Tellurian.Trains.Interfaces.Accessories;

/// <summary>
/// Represents a protocol-agnostic accessory (switch/turnout) address.
/// </summary>
[DataContract]
public readonly struct AccessoryAddress : IEquatable<AccessoryAddress>
{
    /// <summary>
    /// Static method for creating an <see cref="AccessoryAddress"/> from a number.
    /// </summary>
    public static AccessoryAddress From(short accessoryAddress) => 
        new(accessoryAddress);
    /// <summary>
    /// Creates an <see cref="AccessoryAddress"/> from low and high bits.
    /// </summary>
    /// <param name="lowBits"></param>
    /// <param name="highBits"></param>
    /// <returns></returns>
    public static AccessoryAddress From(byte lowBits, byte highBits) => 
        From((short)(lowBits | ((highBits & 0x0F) << 7)));
    /// <summary>
    /// Constructs an <see cref="AccessoryAddress"/> from a number.
    /// </summary>
    /// <param name="number">The accessory address (0-2047).</param>
    private AccessoryAddress(short number)
    {
        Number = number;
    }

    /// <summary>
    /// Tests if an accessory address is valid. Valid range is 0-2047.
    /// </summary>
    /// <param name="number">The address value to verify.</param>
    /// <returns>True if valid; otherwise false.</returns>
    public static bool IsValid(short number) => number >= 0 && number <= 2047;

    /// <summary>
    /// The accessory address value.
    /// </summary>
    [field: DataMember(Name = "number")]
    public short Number
    {
        get;
        init => field = IsValid(value) ? value : throw new ArgumentOutOfRangeException(nameof(value), "Address must be 0-2047.");
    }

    public bool Equals(AccessoryAddress other) => other.Number == Number;
    public override bool Equals(object? obj) => obj is AccessoryAddress other && Equals(other);
    public override int GetHashCode() => Number.GetHashCode();
    public override string ToString() => $"{Number}";
    public static bool operator ==(AccessoryAddress left, AccessoryAddress right) => left.Equals(right);
    public static bool operator !=(AccessoryAddress left, AccessoryAddress right) => !(left == right);
}
