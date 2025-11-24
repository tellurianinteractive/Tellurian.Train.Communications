using System.Runtime.Serialization;

namespace Tellurian.Trains.Interfaces.Locos;

[DataContract]
public readonly struct LocoAddress : IEquatable<LocoAddress>
{
    /// <summary>
    /// Static method for creating a <see cref="LocoAddress"/> from a number.
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    public static LocoAddress From(short number) => new (number);

    [DataMember(Name = "number")]
    private readonly short _Number;

    /// <summary>
    /// Contructs a <see cref="LocoAddress"/> from  number.
    /// </summary>
    /// <param name="number"></param>
    public LocoAddress(short number)
    {
        if (!IsValid(number)) throw new ArgumentOutOfRangeException(nameof(number));
        _Number = number;
    }
    /// <summary>
    /// Tests if a locomotive address is valid or not. A valid address shoukd be in the range 1 - 9999.
    /// </summary>
    /// <param name="number">The adress value to verify.</param>
    /// <returns>True if valid; otherwise false</returns>
    public static bool IsValid(short number) => number >= 1 && number <= 9999;
    /// <summary>
    /// The loco adress is 128 or above.
    /// </summary>
    public bool IsLong => _Number >= 128;
    /// <summary>
    /// The loco adress is max 127.
    /// </summary>
    public bool IsShort => _Number < 128;
    /// <summary>
    /// The adresses 100 to 127 can sometimes cause trouble because some systems regards 1-99 as short and some 1-127.
    /// </summary>
    public bool IsShortTwoDigit => IsShort && _Number < 100;
    /// <summary>
    /// The adresses 100 to 127 can sometimes cause trouble because some systems regards 1-99 as short and some 1-127.
    /// </summary>
    public bool IsShortThreeDigit => IsShort && _Number >= 100;
    /// <summary>
    /// The address.
    /// </summary>
    public short Number => _Number;

    public bool Equals(LocoAddress other) => other.Number == Number;
    public override bool Equals(object? obj)=>  obj is LocoAddress other && Equals(other);
    public override int GetHashCode() => Number.GetHashCode();
    public override string ToString() => $"{Number}";
    public static bool operator ==(LocoAddress left, LocoAddress right) => left.Equals(right);
    public static bool operator !=(LocoAddress left, LocoAddress right) => !(left == right);
}
