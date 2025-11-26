using System.Globalization;
using Tellurian.Trains.Interfaces.Extensions;
using Tellurian.Trains.Protocols.LocoNet.Programming;

namespace Tellurian.Trains.Protocols.LocoNet.Notifications;

/// <summary>
/// OPC_SL_RD_DATA (0xE7) - Slot data read notification.
/// Response from Master containing complete slot information (14 bytes).
/// Sent in response to OPC_LOCO_ADR, OPC_RQ_SL_DATA, OPC_MOVE_SLOTS, etc.
/// Special slots: 123=fast clock, 124=programming track.
/// </summary>
public sealed class SlotNotification : Notification
{
    public const byte OperationCode = 0xE7;
    public const byte ProgrammingSlotNumber = 0x7C; // Slot 124
    public const byte FastClockSlotNumber = 0x7B; // Slot 123

    private readonly byte[] _rawData;
    private readonly Lazy<SlotData> _slotData;
    private readonly Lazy<ProgrammingResult?> _programmingResult;

    internal SlotNotification(byte[] data)
    {
        if (data is null || data.Length != 14)
            throw new ArgumentException("Slot notification must be exactly 14 bytes", nameof(data));

        ValidateData(OperationCode, data);
        _rawData = data;
        _slotData = new Lazy<SlotData>(() => SlotData.FromBytes(data));
        _programmingResult = new Lazy<ProgrammingResult?>(() =>
        {
            if (data[2] == ProgrammingSlotNumber)
                return ProgrammingResult.FromSlotData(data);
            return null;
        });
    }

    /// <summary>
    /// Parsed slot data structure containing all slot information.
    /// </summary>
    public SlotData Data => _slotData.Value;

    /// <summary>
    /// Slot number (0-127). Shortcut to Data.SlotNumber.
    /// </summary>
    public byte SlotNumber => Data.SlotNumber;

    /// <summary>
    /// True if this is a programming track response (slot 124).
    /// </summary>
    public bool IsProgrammingSlot => SlotNumber == ProgrammingSlotNumber;

    /// <summary>
    /// True if this is a fast clock response (slot 123).
    /// </summary>
    public bool IsFastClockSlot => SlotNumber == FastClockSlotNumber;

    /// <summary>
    /// True if this is a regular locomotive slot (not special).
    /// </summary>
    public bool IsLocomotiveSlot => !IsProgrammingSlot && !IsFastClockSlot;

    /// <summary>
    /// Programming result if this is slot 124, otherwise null.
    /// Check IsProgrammingSlot before accessing.
    /// </summary>
    public ProgrammingResult? ProgrammingResult => _programmingResult.Value;

    /// <summary>
    /// Locomotive address (1-9999). Shortcut to Data.Address.
    /// Only meaningful for locomotive slots.
    /// </summary>
    public ushort Address => Data.Address;

    /// <summary>
    /// Current speed (0-127). Shortcut to Data.Speed.
    /// Only meaningful for locomotive slots.
    /// </summary>
    public byte Speed => Data.Speed;

    /// <summary>
    /// Direction: true=forward, false=reverse. Shortcut to Data.Direction.
    /// Only meaningful for locomotive slots.
    /// </summary>
    public bool Direction => Data.Direction;

    /// <summary>
    /// Slot status (FREE, COMMON, IDLE, IN_USE). Shortcut to Data.Status.
    /// </summary>
    public SlotStatus Status => Data.Status;

    /// <summary>
    /// Gets the raw 14-byte message data.
    /// </summary>
    public byte[] RawData => _rawData;

    public override string ToString()
    {
        if (IsProgrammingSlot && ProgrammingResult != null)
            return string.Format(CultureInfo.CurrentCulture,
                "Programming: {0}", ProgrammingResult);

        if (IsFastClockSlot)
            return "Fast Clock Slot";

        return string.Format(CultureInfo.CurrentCulture,
            "Slot {0}: Addr={1}, Speed={2}, Dir={3}, Status={4}",
            SlotNumber, Address, Speed, Direction ? "FWD" : "REV", Status);
    }
}
