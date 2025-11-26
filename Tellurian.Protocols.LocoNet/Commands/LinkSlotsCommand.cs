namespace Tellurian.Trains.Protocols.LocoNet.Commands;

/// <summary>
/// OPC_LINK_SLOTS (0xB9) - Link two slots for consisting.
/// Slaves slot1 to slot2 so that slot1 follows slot2's speed and direction.
/// Used to create multi-locomotive consists (lash-ups).
/// Response: OPC_SL_RD_DATA or OPC_LONG_ACK on failure.
/// </summary>
public sealed class LinkSlotsCommand : Command
{
    public const byte OperationCode = 0xB9;

    public LinkSlotsCommand(byte slaveSlot, byte masterSlot)
    {
        if (slaveSlot > 127)
            throw new ArgumentOutOfRangeException(nameof(slaveSlot), "Slot number must be 0-127");
        if (masterSlot > 127)
            throw new ArgumentOutOfRangeException(nameof(masterSlot), "Slot number must be 0-127");
        if (slaveSlot == masterSlot)
            throw new ArgumentException("Slave and master slots must be different");

        SlaveSlot = slaveSlot;
        MasterSlot = masterSlot;
    }

    /// <summary>
    /// Slave slot number (will follow the master).
    /// </summary>
    public byte SlaveSlot { get; }

    /// <summary>
    /// Master slot number (controls the consist).
    /// </summary>
    public byte MasterSlot { get; }

    /// <summary>
    /// Generates the 4-byte message: [0xB9, slave, master, checksum].
    /// After linking, slave slot will have SL_CONUP flag set, master will have SL_CONDN flag set.
    /// </summary>
    public override byte[] GetBytesWithChecksum()
    {
        return AppendChecksum([OperationCode, SlaveSlot, MasterSlot]);
    }

    public override string ToString()
    {
        return $"Link Slot {SlaveSlot} -> Slot {MasterSlot}";
    }
}
