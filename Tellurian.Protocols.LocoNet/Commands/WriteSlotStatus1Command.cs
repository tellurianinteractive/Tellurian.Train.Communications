namespace Tellurian.Trains.Protocols.LocoNet.Commands;

/// <summary>
/// OPC_SLOT_STAT1 (0xB5) - Write slot status1 byte.
/// Changes only the STATUS1 byte of a slot (faster than writing entire slot).
/// Use cases:
/// - Mark slot as COMMON vs IN_USE
/// - Change decoder type
/// - Modify consist flags
/// Response: OPC_SL_RD_DATA or OPC_LONG_ACK
/// </summary>
public sealed class WriteSlotStatus1Command : Command
{
    public const byte OperationCode = 0xB5;

    public WriteSlotStatus1Command(byte slotNumber, byte status1)
    {
        if (slotNumber > 127)
            throw new ArgumentOutOfRangeException(nameof(slotNumber), "Slot number must be 0-127");

        SlotNumber = slotNumber;
        Status1 = status1;
    }

    /// <summary>
    /// Creates a command to change slot status.
    /// </summary>
    public WriteSlotStatus1Command(byte slotNumber, SlotStatus status, ConsistStatus consist, DecoderType decoderType)
    {
        if (slotNumber > 127)
            throw new ArgumentOutOfRangeException(nameof(slotNumber), "Slot number must be 0-127");

        SlotNumber = slotNumber;

        // Build STAT1 byte from components
        Status1 = (byte)(
            ((byte)status << 4) |
            ((byte)consist & 0b1000) |      // CONUP bit
            (((byte)consist & 0b0001) << 3) | // CONDN bit
            (byte)decoderType
        );
    }

    /// <summary>
    /// Slot number to modify (0-127).
    /// </summary>
    public byte SlotNumber { get; }

    /// <summary>
    /// New STATUS1 byte value.
    /// </summary>
    public byte Status1 { get; }

    /// <summary>
    /// Slot status extracted from STATUS1.
    /// </summary>
    public SlotStatus Status => (SlotStatus)((Status1 >> 4) & 0b11);

    /// <summary>
    /// Consist status extracted from STATUS1.
    /// </summary>
    public ConsistStatus Consist =>
        (ConsistStatus)(((Status1 >> 3) & 0b1000) | ((Status1 >> 3) & 0b0001));

    /// <summary>
    /// Decoder type extracted from STATUS1.
    /// </summary>
    public DecoderType DecoderType => (DecoderType)(Status1 & 0b111);

    /// <summary>
    /// Generates the 4-byte message: [0xB5, slot, stat1, checksum].
    /// </summary>
    public override byte[] GetBytesWithChecksum()
    {
        return AppendChecksum([OperationCode, SlotNumber, Status1]);
    }

    /// <summary>
    /// Creates a command to change only the slot status (keeping consist and decoder type unchanged).
    /// Note: This requires knowing the current consist and decoder type values.
    /// </summary>
    public static WriteSlotStatus1Command ChangeStatus(
        byte slotNumber,
        SlotStatus newStatus,
        ConsistStatus currentConsist = ConsistStatus.NotInConsist,
        DecoderType currentDecoderType = DecoderType.Steps128)
    {
        return new WriteSlotStatus1Command(slotNumber, newStatus, currentConsist, currentDecoderType);
    }
}
