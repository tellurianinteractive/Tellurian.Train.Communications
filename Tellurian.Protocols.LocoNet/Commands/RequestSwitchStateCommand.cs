namespace Tellurian.Trains.Protocols.LocoNet.Commands;

/// <summary>
/// OPC_SW_STATE (0xBC) - Request switch state.
/// Requests the current state of a specific switch/turnout.
/// Response: OPC_SW_REP (0xB1) with current switch state, or OPC_LONG_ACK.
/// </summary>
public sealed class RequestSwitchStateCommand : Command
{
    public const byte OperationCode = 0xBC;

    public RequestSwitchStateCommand(ushort address)
    {
        if (address > 2047)
            throw new ArgumentOutOfRangeException(nameof(address), "Switch address must be 0-2047");

        Address = new AccessoryAddress(address);
    }

    public RequestSwitchStateCommand(AccessoryAddress address)
    {
        Address = address;
    }

    /// <summary>
    /// The accessory address to query (0-2047).
    /// </summary>
    public AccessoryAddress Address { get; }

    /// <summary>
    /// Generates the 4-byte message: [0xBC, sw1, sw2, checksum].
    /// The DIR and ON bits in sw2 should be set appropriately for the query.
    /// </summary>
    public override byte[] GetBytesWithChecksum()
    {
        // For state request, we typically use DIR=0, ON=0
        var (sw1, sw2) = Address.EncodeSwitchBytes(AccessoryFunction.ThrownOrRed, OutputState.Off);
        return AppendChecksum([OperationCode, sw1, sw2]);
    }
}
