using Tellurian.Trains.Interfaces.Locos;

namespace Tellurian.Trains.Protocols.XpressNet.Commands;

/// <summary>
/// Function status request (spec section 4.19.4).
/// Requests the current function status (momentary vs on/off) for a locomotive.
/// </summary>
/// <remarks>
/// Format: Header=0xE3, Data=[0x07, AH, AL]
///
/// XpressNet supports the concept of both momentary functions and on/off functions.
/// The command station maintains the status of each function. This request queries
/// the current function status for functions F0-F12 for a particular locomotive.
///
/// Response: FunctionStatusNotification (section 3.16)
/// </remarks>
public sealed class GetFunctionStatusCommand : Command
{
    /// <summary>
    /// Creates a function status request command.
    /// </summary>
    /// <param name="address">Locomotive address (1-9999)</param>
    public GetFunctionStatusCommand(Address address)
        : base(0xE3, GetData(address)) { }

    private static byte[] GetData(Address address)
    {
        var addrBytes = address.GetBytesAccordingToXpressNet();
        return [0x07, addrBytes[0], addrBytes[1]];
    }
}
