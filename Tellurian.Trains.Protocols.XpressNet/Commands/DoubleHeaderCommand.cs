using Tellurian.Trains.Interfaces.Locos;

namespace Tellurian.Trains.Protocols.XpressNet.Commands;

/// <summary>
/// Base class for Double Header operations (spec section 4.22).
/// Double Headers join two locomotives so that speed and direction commands
/// sent to one are sent to both by the command station.
/// </summary>
public abstract class DoubleHeaderCommand : Command
{
    protected DoubleHeaderCommand(Address address1, Address address2)
        : base(0xE5, GetData(address1, address2)) { }

    private static byte[] GetData(Address address1, Address address2)
    {
        var addr1Bytes = address1.GetBytesAccordingToXpressNet();
        var addr2Bytes = address2.GetBytesAccordingToXpressNet();
        return
        [
            0x43,           // Identification
            addr1Bytes[0],  // Address 1 High
            addr1Bytes[1],  // Address 1 Low
            addr2Bytes[0],  // Address 2 High
            addr2Bytes[1]   // Address 2 Low
        ];
    }
}

/// <summary>
/// Establish Double Header command (spec section 4.22.1).
/// Joins two locomotives into a Double Header.
/// </summary>
/// <remarks>
/// Format: Header=0xE5, Data=[0x43, AH1, AL1, AH2, AL2]
///
/// Once established, speed and direction commands sent to either locomotive address
/// are sent to both locomotives by the command station.
///
/// Locomotives with addresses 0-9999 can be controlled.
///
/// Response: If not successful, the command station sends MUDHErrorNotification (section 3.21).
/// </remarks>
public sealed class EstablishDoubleHeaderCommand : DoubleHeaderCommand
{
    /// <summary>
    /// Creates a command to establish a Double Header between two locomotives.
    /// </summary>
    /// <param name="address1">First locomotive address (1-9999)</param>
    /// <param name="address2">Second locomotive address (1-9999)</param>
    public EstablishDoubleHeaderCommand(Address address1, Address address2)
        : base(address1, address2) { }
}

/// <summary>
/// Dissolve Double Header command (spec section 4.22.2).
/// Removes a locomotive from a Double Header, dissolving the consist.
/// </summary>
/// <remarks>
/// Format: Header=0xE5, Data=[0x43, AH1, AL1, 0x00, 0x00]
///
/// The command station recognizes this as a dissolve command by the value of 0
/// in the second locomotive address. The locomotive specified is removed from
/// its Double Header, dissolving the consist.
///
/// Response: If not successful, the command station sends MUDHErrorNotification (section 3.21).
/// </remarks>
public sealed class DissolveDoubleHeaderCommand : Command
{
    /// <summary>
    /// Creates a command to dissolve a Double Header by removing a locomotive.
    /// </summary>
    /// <param name="address">Locomotive address to remove from Double Header (1-9999)</param>
    public DissolveDoubleHeaderCommand(Address address)
        : base(0xE5, GetData(address)) { }

    private static byte[] GetData(Address address)
    {
        var addrBytes = address.GetBytesAccordingToXpressNet();
        return
        [
            0x43,           // Identification
            addrBytes[0],   // Address High
            addrBytes[1],   // Address Low
            0x00,           // Second address = 0 indicates dissolve
            0x00
        ];
    }
}
