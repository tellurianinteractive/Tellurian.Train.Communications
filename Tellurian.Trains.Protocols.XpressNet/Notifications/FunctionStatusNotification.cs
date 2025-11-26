namespace Tellurian.Trains.Protocols.XpressNet.Notifications;

/// <summary>
/// Function status response (spec section 3.16).
/// Provides the function state (momentary vs on/off) for functions F0-F12.
/// </summary>
/// <remarks>
/// Format: Header=0xE3, Data=[0x50, S0, S1]
/// - S0 = 000S0S4S3S2S1 - Status of F0-F4 (Sx=1 means on/off, Sx=0 means momentary)
/// - S1 = S12S11S10S9S8S7S6S5 - Status of F5-F12 (Sx=1 means on/off, Sx=0 means momentary)
///
/// This response is provided in response to a GetFunctionStatusCommand.
/// The associated locomotive address is not sent in the response, because it
/// follows a specific request that includes the locomotive address.
///
/// Note: The concept of momentary function does not change the DCC packets to the track.
/// A momentary function is still implemented as an ON operation followed by an OFF operation.
/// Instead this feature lets the XpressNet device extend its functionality in the operator interface.
/// For example, if F5 is assigned to Horn and defined as momentary, the XpressNet device
/// can send an ON operation when the F5 key is pressed and an OFF operation when released.
/// </remarks>
public sealed class FunctionStatusNotification : Notification
{
    internal FunctionStatusNotification(byte[] buffer) : base(0xE3, GetData(buffer)) { }

    private static new byte[] GetData(byte[] buffer)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        if (buffer.Length < 4) throw new ArgumentOutOfRangeException(nameof(buffer), "Buffer must contain at least 4 bytes");
        return [buffer[1], buffer[2], buffer[3]];
    }

    /// <summary>
    /// Gets the raw status byte for Group 1 (F0-F4).
    /// Format: 000S0S4S3S2S1
    /// </summary>
    public byte Group1Status => Data[1];

    /// <summary>
    /// Gets the raw status byte for Groups 2 and 3 (F5-F12).
    /// Format: S12S11S10S9S8S7S6S5
    /// </summary>
    public byte Group2And3Status => Data[2];

    /// <summary>
    /// Gets whether a function is on/off (true) or momentary (false).
    /// </summary>
    /// <param name="functionNumber">Function number (0-12)</param>
    /// <returns>True if the function is on/off, false if momentary</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when function number is not 0-12</exception>
    public bool IsFunctionOnOff(int functionNumber) => functionNumber switch
    {
        0 => (Group1Status & 0x10) != 0,
        1 => (Group1Status & 0x01) != 0,
        2 => (Group1Status & 0x02) != 0,
        3 => (Group1Status & 0x04) != 0,
        4 => (Group1Status & 0x08) != 0,
        5 => (Group2And3Status & 0x01) != 0,
        6 => (Group2And3Status & 0x02) != 0,
        7 => (Group2And3Status & 0x04) != 0,
        8 => (Group2And3Status & 0x08) != 0,
        9 => (Group2And3Status & 0x10) != 0,
        10 => (Group2And3Status & 0x20) != 0,
        11 => (Group2And3Status & 0x40) != 0,
        12 => (Group2And3Status & 0x80) != 0,
        _ => throw new ArgumentOutOfRangeException(nameof(functionNumber), "Function number must be 0-12")
    };

    /// <summary>
    /// Gets whether a function is momentary.
    /// </summary>
    /// <param name="functionNumber">Function number (0-12)</param>
    /// <returns>True if the function is momentary, false if on/off</returns>
    public bool IsFunctionMomentary(int functionNumber) => !IsFunctionOnOff(functionNumber);

    /// <summary>
    /// Gets the function states as a boolean array indexed by function number.
    /// Value is true for on/off, false for momentary.
    /// </summary>
    public bool[] GetAllFunctionStates() =>
        Enumerable.Range(0, 13).Select(IsFunctionOnOff).ToArray();
}
