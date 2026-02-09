using Tellurian.Trains.Communications.Interfaces.Accessories;

namespace Tellurian.Trains.Protocols.LocoNet.Commands;

/// <summary>
/// OPC_SW_STATE (0xBC) - Request accessory state.
/// Requests the current state of a specific accessory.
/// Response: OPC_SW_REP (0xB1) with current accessory state, or OPC_LONG_ACK.
/// </summary>
public sealed class RequestAccessoryStateCommand : Command
{
    public const byte OperationCode = 0xBC;

    public RequestAccessoryStateCommand(Address address)
    {
        Address = address;
    }

    /// <summary>
    /// The accessory address to query (0-2047).
    /// </summary>
    public Address Address { get; }

    /// <summary>
    /// Generates the 4-byte message: [0xBC, sw1, sw2, checksum].
    /// The DIR and ON bits in sw2 should be set appropriately for the query.
    /// </summary>
    public override byte[] GetBytesWithChecksum()
    {
        // For state request, we typically use DIR=0, ON=0
        var (sw1, sw2) = Address.EncodeAccessoryBytes(Position.ThrownOrRed, MotorState.Off);
        return AppendChecksum([OperationCode, sw1, sw2]);
    }
}
