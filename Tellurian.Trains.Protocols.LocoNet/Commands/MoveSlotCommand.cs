namespace Tellurian.Trains.Protocols.LocoNet.Commands;

/// <summary>
/// OPC_MOVE_SLOTS (0xBA) - Move/dispatch/activate slot.
/// Multi-function command for slot management with several special cases.
/// Response: OPC_SL_RD_DATA with destination slot info, or OPC_LONG_ACK on failure.
/// </summary>
public sealed class MoveSlotCommand : Command
{
    public const byte OperationCode = 0xBA;

    public MoveSlotCommand(byte sourceSlot, byte destinationSlot)
    {
        if (sourceSlot > 127)
            throw new ArgumentOutOfRangeException(nameof(sourceSlot), "Slot number must be 0-127");
        if (destinationSlot > 127)
            throw new ArgumentOutOfRangeException(nameof(destinationSlot), "Slot number must be 0-127");

        SourceSlot = sourceSlot;
        DestinationSlot = destinationSlot;
    }

    /// <summary>
    /// Source slot number (0-127).
    /// </summary>
    public byte SourceSlot { get; }

    /// <summary>
    /// Destination slot number (0-127).
    /// </summary>
    public byte DestinationSlot { get; }

    /// <summary>
    /// Creates a NULL move to activate a slot (mark as IN_USE).
    /// This is required after requesting a locomotive address before you can control it.
    /// </summary>
    /// <param name="slotNumber">Slot to activate</param>
    public static MoveSlotCommand Activate(byte slotNumber)
    {
        return new MoveSlotCommand(slotNumber, slotNumber);
    }

    /// <summary>
    /// Creates a DISPATCH PUT command to put a slot into the dispatch stack.
    /// This releases control and makes the locomotive available for other throttles.
    /// </summary>
    /// <param name="slotNumber">Slot to dispatch</param>
    public static MoveSlotCommand DispatchPut(byte slotNumber)
    {
        return new MoveSlotCommand(slotNumber, 0);
    }

    /// <summary>
    /// Creates a DISPATCH GET command to retrieve the next dispatched locomotive.
    /// Response will contain the slot number of the dispatched locomotive.
    /// </summary>
    public static MoveSlotCommand DispatchGet()
    {
        return new MoveSlotCommand(0, 0);
    }

    /// <summary>
    /// Generates the 4-byte message: [0xBA, src, dest, checksum].
    /// Special cases:
    /// - NULL move (src=dest): Activates slot
    /// - DISPATCH PUT (dest=0): Puts slot in dispatch stack
    /// - DISPATCH GET (src=0, dest=0): Gets dispatched locomotive
    /// </summary>
    public override byte[] GetBytesWithChecksum()
    {
        return AppendChecksum([OperationCode, SourceSlot, DestinationSlot]);
    }
}
