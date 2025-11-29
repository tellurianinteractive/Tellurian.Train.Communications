using Tellurian.Trains.Interfaces.Locos;

namespace Tellurian.Trains.Protocols.XpressNet.Commands;

/// <summary>
/// Set function state commands (spec section 4.20.5).
/// Sets the function state (momentary vs on/off) for locomotive functions.
/// </summary>
/// <remarks>
/// XpressNet allows functions F0-F12 to be specified as either momentary or constant on/off.
/// Momentary functions are especially useful for sound control (e.g., horn).
///
/// The state is stored in the command station database.
/// It is up to the XpressNet device to determine the length of time a momentary function is on.
///
/// Functions are grouped into three sets:
/// - Group 1: F0-F4
/// - Group 2: F5-F8
/// - Group 3: F9-F12
/// </remarks>
public abstract class SetFunctionStateCommand : Command
{
    protected SetFunctionStateCommand(byte identification, Address address, byte stateByte)
        : base(0xE4, GetData(identification, address, stateByte)) { }

    private static byte[] GetData(byte identification, Address address, byte stateByte)
    {
        var addrBytes = address.GetBytesAccordingToXpressNet();
        return [identification, addrBytes[0], addrBytes[1], stateByte];
    }
}

/// <summary>
/// Set function state for Group 1 (F0-F4) (spec section 4.20.5).
/// </summary>
/// <remarks>
/// Format: Header=0xE4, Data=[0x24, AH, AL, 000S0S4S3S2S1]
/// Where Sx=1 means on/off, Sx=0 means momentary.
/// </remarks>
public sealed class SetFunctionStateGroup1Command : SetFunctionStateCommand
{
    /// <summary>
    /// Creates a set function state command for Group 1 (F0-F4).
    /// </summary>
    /// <param name="address">Locomotive address (1-9999)</param>
    /// <param name="f0OnOff">True if F0 is on/off, false if momentary</param>
    /// <param name="f1OnOff">True if F1 is on/off, false if momentary</param>
    /// <param name="f2OnOff">True if F2 is on/off, false if momentary</param>
    /// <param name="f3OnOff">True if F3 is on/off, false if momentary</param>
    /// <param name="f4OnOff">True if F4 is on/off, false if momentary</param>
    public SetFunctionStateGroup1Command(Address address,
        bool f0OnOff = true, bool f1OnOff = true, bool f2OnOff = true,
        bool f3OnOff = true, bool f4OnOff = true)
        : base(0x24, address, CreateStateByte(f0OnOff, f1OnOff, f2OnOff, f3OnOff, f4OnOff)) { }

    /// <summary>
    /// Creates a set function state command for Group 1 using a state byte.
    /// </summary>
    /// <param name="address">Locomotive address (1-9999)</param>
    /// <param name="stateByte">State byte: 000S0S4S3S2S1 (bit=1 means on/off, bit=0 means momentary)</param>
    public SetFunctionStateGroup1Command(Address address, byte stateByte)
        : base(0x24, address, (byte)(stateByte & 0x1F)) { }

    private static byte CreateStateByte(bool f0OnOff, bool f1OnOff, bool f2OnOff, bool f3OnOff, bool f4OnOff)
    {
        byte state = 0;
        if (f0OnOff) state |= 0x10;
        if (f1OnOff) state |= 0x01;
        if (f2OnOff) state |= 0x02;
        if (f3OnOff) state |= 0x04;
        if (f4OnOff) state |= 0x08;
        return state;
    }
}

/// <summary>
/// Set function state for Group 2 (F5-F8) (spec section 4.20.5).
/// </summary>
/// <remarks>
/// Format: Header=0xE4, Data=[0x25, AH, AL, 0000S8S7S6S5]
/// Where Sx=1 means on/off, Sx=0 means momentary.
/// </remarks>
public sealed class SetFunctionStateGroup2Command : SetFunctionStateCommand
{
    /// <summary>
    /// Creates a set function state command for Group 2 (F5-F8).
    /// </summary>
    /// <param name="address">Locomotive address (1-9999)</param>
    /// <param name="f5OnOff">True if F5 is on/off, false if momentary</param>
    /// <param name="f6OnOff">True if F6 is on/off, false if momentary</param>
    /// <param name="f7OnOff">True if F7 is on/off, false if momentary</param>
    /// <param name="f8OnOff">True if F8 is on/off, false if momentary</param>
    public SetFunctionStateGroup2Command(Address address,
        bool f5OnOff = true, bool f6OnOff = true, bool f7OnOff = true, bool f8OnOff = true)
        : base(0x25, address, CreateStateByte(f5OnOff, f6OnOff, f7OnOff, f8OnOff)) { }

    /// <summary>
    /// Creates a set function state command for Group 2 using a state byte.
    /// </summary>
    /// <param name="address">Locomotive address (1-9999)</param>
    /// <param name="stateByte">State byte: 0000S8S7S6S5 (bit=1 means on/off, bit=0 means momentary)</param>
    public SetFunctionStateGroup2Command(Address address, byte stateByte)
        : base(0x25, address, (byte)(stateByte & 0x0F)) { }

    private static byte CreateStateByte(bool f5OnOff, bool f6OnOff, bool f7OnOff, bool f8OnOff)
    {
        byte state = 0;
        if (f5OnOff) state |= 0x01;
        if (f6OnOff) state |= 0x02;
        if (f7OnOff) state |= 0x04;
        if (f8OnOff) state |= 0x08;
        return state;
    }
}

/// <summary>
/// Set function state for Group 3 (F9-F12) (spec section 4.20.5).
/// </summary>
/// <remarks>
/// Format: Header=0xE4, Data=[0x26, AH, AL, 0000S12S11S10S9]
/// Where Sx=1 means on/off, Sx=0 means momentary.
/// </remarks>
public sealed class SetFunctionStateGroup3Command : SetFunctionStateCommand
{
    /// <summary>
    /// Creates a set function state command for Group 3 (F9-F12).
    /// </summary>
    /// <param name="address">Locomotive address (1-9999)</param>
    /// <param name="f9OnOff">True if F9 is on/off, false if momentary</param>
    /// <param name="f10OnOff">True if F10 is on/off, false if momentary</param>
    /// <param name="f11OnOff">True if F11 is on/off, false if momentary</param>
    /// <param name="f12OnOff">True if F12 is on/off, false if momentary</param>
    public SetFunctionStateGroup3Command(Address address,
        bool f9OnOff = true, bool f10OnOff = true, bool f11OnOff = true, bool f12OnOff = true)
        : base(0x26, address, CreateStateByte(f9OnOff, f10OnOff, f11OnOff, f12OnOff)) { }

    /// <summary>
    /// Creates a set function state command for Group 3 using a state byte.
    /// </summary>
    /// <param name="address">Locomotive address (1-9999)</param>
    /// <param name="stateByte">State byte: 0000S12S11S10S9 (bit=1 means on/off, bit=0 means momentary)</param>
    public SetFunctionStateGroup3Command(Address address, byte stateByte)
        : base(0x26, address, (byte)(stateByte & 0x0F)) { }

    private static byte CreateStateByte(bool f9OnOff, bool f10OnOff, bool f11OnOff, bool f12OnOff)
    {
        byte state = 0;
        if (f9OnOff) state |= 0x01;
        if (f10OnOff) state |= 0x02;
        if (f11OnOff) state |= 0x04;
        if (f12OnOff) state |= 0x08;
        return state;
    }
}
