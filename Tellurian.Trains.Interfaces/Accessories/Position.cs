namespace Tellurian.Trains.Interfaces.Accessories;

/// <summary>
/// Represents the function/position of an accessory (switch/turnout).
/// </summary>
#pragma warning disable CA1028 // Enum Storage should be Int32
public enum Position : byte
{
    /// <summary>
    /// Switch is closed (straight) or signal shows green.
    /// </summary>
    ClosedOrGreen = 0,
    /// <summary>
    /// Switch is thrown (diverging) or signal shows red.
    /// </summary>
    ThrownOrRed = 1
}
