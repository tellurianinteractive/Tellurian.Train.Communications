namespace Tellurian.Trains.Protocols.LocoNet.Commands;

/// <summary>
/// OPC_WR_SL_DATA (0xEF) - Write slot data.
/// Writes complete slot data (14 bytes total). Used for:
/// - Modifying multiple slot parameters at once
/// - Programming track operations (slot 124)
/// - Fast clock control (slot 123)
/// Response: OPC_LONG_ACK
/// </summary>
public sealed class WriteSlotDataCommand : Command
{
    public const byte OperationCode = 0xEF;

    private readonly SlotData _slotData;

    /// <summary>
    /// Creates a write slot data command from a SlotData structure.
    /// </summary>
    /// <param name="slotData">The slot data to write</param>
    public WriteSlotDataCommand(SlotData slotData)
    {
        _slotData = slotData ?? throw new ArgumentNullException(nameof(slotData));
    }

    /// <summary>
    /// The slot data to be written.
    /// </summary>
    public SlotData Data => _slotData;

    /// <summary>
    /// Generates the 14-byte message: [0xEF, 0x0E, slot_data..., checksum].
    /// </summary>
    public override byte[] GetBytesWithChecksum()
    {
        byte[] data = _slotData.ToBytes(OperationCode);
        return AppendChecksum(data);
    }

    /// <summary>
    /// Creates a WriteSlotDataCommand by modifying an existing SlotData.
    /// This is useful when you want to change specific fields without rebuilding the entire structure.
    /// </summary>
    /// <param name="existingData">The existing slot data to modify</param>
    /// <param name="speed">New speed value (null to keep existing)</param>
    /// <param name="direction">New direction (null to keep existing)</param>
    /// <param name="status">New status (null to keep existing)</param>
    /// <returns>WriteSlotDataCommand with modified data</returns>
    public static WriteSlotDataCommand ModifySlot(
        SlotData existingData,
        byte? speed = null,
        bool? direction = null,
        SlotStatus? status = null)
    {
        var modifiedData = new SlotData
        {
            SlotNumber = existingData.SlotNumber,
            Address = existingData.Address,
            Speed = speed ?? existingData.Speed,
            Direction = direction ?? existingData.Direction,
            Status = status ?? existingData.Status,
            Consist = existingData.Consist,
            DecoderType = existingData.DecoderType,
            TrackStatus = existingData.TrackStatus,
            F0 = existingData.F0,
            F1 = existingData.F1,
            F2 = existingData.F2,
            F3 = existingData.F3,
            F4 = existingData.F4,
            F5 = existingData.F5,
            F6 = existingData.F6,
            F7 = existingData.F7,
            F8 = existingData.F8,
            DeviceId = existingData.DeviceId,
            Status2 = existingData.Status2
        };

        return new WriteSlotDataCommand(modifiedData);
    }
}
