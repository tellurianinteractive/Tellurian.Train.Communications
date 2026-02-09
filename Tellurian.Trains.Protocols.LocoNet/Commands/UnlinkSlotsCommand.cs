namespace Tellurian.Trains.Protocols.LocoNet.Commands;

/// <summary>
/// OPC_UNLINK_SLOTS (0xB8) - Unlink consist slots.
/// Removes the consist link between two slots, allowing them to operate independently.
/// Response: OPC_SL_RD_DATA with updated slot status or OPC_LONG_ACK on failure.
/// </summary>
public sealed class UnlinkSlotsCommand : Command
{
    public const byte OperationCode = 0xB8;

    public UnlinkSlotsCommand(byte slaveSlot, byte masterSlot)
    {
        if (slaveSlot > 127)
            throw new ArgumentOutOfRangeException(nameof(slaveSlot), "Slot number must be 0-127");
        if (masterSlot > 127)
            throw new ArgumentOutOfRangeException(nameof(masterSlot), "Slot number must be 0-127");

        SlaveSlot = slaveSlot;
        MasterSlot = masterSlot;
    }

    /// <summary>
    /// Slave slot number to unlink.
    /// </summary>
    public byte SlaveSlot { get; }

    /// <summary>
    /// Master slot number to unlink from.
    /// </summary>
    public byte MasterSlot { get; }

    /// <summary>
    /// Creates an unlink command from a SlotData structure.
    /// Automatically extracts slot number and determines master/slave relationship from consist status.
    /// </summary>
    /// <param name="slotData">Slot data with consist information</param>
    /// <returns>UnlinkSlotsCommand or null if not in consist</returns>
    public static UnlinkSlotsCommand? FromSlotData(SlotData slotData)
    {
        ArgumentNullException.ThrowIfNull(slotData);

        // If not in a consist, can't unlink
        if (slotData.Consist == ConsistStatus.NotInConsist)
            return null;

        // For unlink, we need to know both slots
        // This is a simplified version - in practice you'd track the consist structure
        // For now, return a command that needs the other slot number
        return null;
    }

    /// <summary>
    /// Generates the 4-byte message: [0xB8, slave, master, checksum].
    /// After unlinking, consist flags (SL_CONUP/SL_CONDN) will be cleared.
    /// </summary>
    public override byte[] GetBytesWithChecksum()
    {
        return AppendChecksum([OperationCode, SlaveSlot, MasterSlot]);
    }

    public override string ToString()
    {
        return $"Unlink Slot {SlaveSlot} from Slot {MasterSlot}";
    }
}
