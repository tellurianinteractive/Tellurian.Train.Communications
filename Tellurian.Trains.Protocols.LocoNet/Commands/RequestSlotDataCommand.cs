namespace Tellurian.Trains.Protocols.LocoNet.Commands;

/// <summary>
/// OPC_RQ_SL_DATA (0xBB) - Request slot data.
/// Explicitly requests the current data for a specific slot number.
/// Response: OPC_SL_RD_DATA (14 bytes) with slot information.
/// </summary>
public sealed class RequestSlotDataCommand : Command
{
    public const byte OperationCode = 0xBB;

    public RequestSlotDataCommand(byte slotNumber)
    {
        if (slotNumber > 127)
            throw new ArgumentOutOfRangeException(nameof(slotNumber), "Slot number must be 0-127");

        SlotNumber = slotNumber;
    }

    /// <summary>
    /// The slot number to request (0-127).
    /// Special slots: 0=dispatch, 123=fast clock, 124=programming.
    /// </summary>
    public byte SlotNumber { get; }

    /// <summary>
    /// Generates the 4-byte message: [0xBB, slot, 0x00, checksum].
    /// </summary>
    public override byte[] GetBytesWithChecksum()
    {
        return AppendChecksum([OperationCode, SlotNumber, 0x00]);
    }
}
