using Tellurian.Trains.Interfaces.Locos;

namespace Tellurian.Trains.Protocols.LocoNet.Commands;

/// <summary>
/// OPC_LOCO_ADR (0xBF) - Request locomotive address.
/// Asks the Master for a slot containing the specified locomotive address.
/// Response: OPC_SL_RD_DATA (14 bytes) with slot information, or OPC_LONG_ACK on failure.
/// </summary>
public sealed class GetLocoAddressCommand : Command
{
    public const byte OperationCode = 0xBF;

    public GetLocoAddressCommand(Address address)
    {
        Address = address;
    }

    /// <summary>
    /// The locomotive address to request (1-9999).
    /// </summary>
    public Address Address { get; }

    /// <summary>
    /// Generates the 4-byte message: [0xBF, adr_hi, adr_lo, checksum].
    /// Short addresses (1-127): adr_hi = 0x00, adr_lo = address
    /// Long addresses (128-9999): 14-bit split across both bytes
    /// </summary>
    public override byte[] GetBytesWithChecksum()
    {
        return AppendChecksum([OperationCode, Address.High, Address.Low]);
    }
}
