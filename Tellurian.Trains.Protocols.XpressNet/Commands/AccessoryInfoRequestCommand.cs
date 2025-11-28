namespace Tellurian.Trains.Protocols.XpressNet.Commands;

/// <summary>
/// Accessory Decoder information request (spec section 4.17).
/// Requests the command station to respond with accessory decoder status.
/// </summary>
/// <remarks>
/// Format: Header=0x42, Data=[Address, 0x80+N]
/// - Address: Group address (turnout address / 4), range 0-255
/// - N: Nibble selector (0=lower nibble for turnouts 0-1, 1=upper nibble for turnouts 2-3)
///
/// For feedback modules, the address is the module address (0-127).
/// </remarks>
public sealed class AccessoryInfoRequestCommand : Command
{
    /// <summary>
    /// Creates an accessory decoder information request for a specific turnout.
    /// </summary>
    /// <param name="address">Turnout address (1-1024)</param>
    public AccessoryInfoRequestCommand(AccessoryAddress address)
        : base(0x42, GetData(address)) { }

    /// <summary>
    /// Creates an accessory decoder information request for a group address and nibble.
    /// </summary>
    /// <param name="groupAddress">Group address (0-255)</param>
    /// <param name="upperNibble">True for upper nibble (turnouts 2-3), false for lower (turnouts 0-1)</param>
    public AccessoryInfoRequestCommand(byte groupAddress, bool upperNibble)
        : base(0x42, GetData(groupAddress, upperNibble)) { }

    private static byte[] GetData(AccessoryAddress address)
    {
        var upperNibble = address.Subaddress >= 2;
        return GetData(address.Group, upperNibble);
    }

    private static byte[] GetData(byte groupAddress, bool upperNibble) =>
    [
        groupAddress,
        (byte)(0x80 + (upperNibble ? 1 : 0))
    ];
}
