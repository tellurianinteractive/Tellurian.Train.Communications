namespace Tellurian.Trains.Protocols.LocoNet.Commands;

/// <summary>
/// OPC_CONSIST_FUNC (0xB6) - Set functions on consist member.
/// Controls direction and functions F0-F4 on a specific consist member without affecting the whole consist.
/// Only works on slots that have SL_CONUP flag set (uplinked consist members).
/// Response: None (fire and forget).
/// </summary>
public sealed class ConsistFunctionCommand : Command
{
    public const byte OperationCode = 0xB6;

    public ConsistFunctionCommand(byte slotNumber, bool forward, bool f0, bool f1, bool f2, bool f3, bool f4)
    {
        if (slotNumber > 127)
            throw new ArgumentOutOfRangeException(nameof(slotNumber), "Slot number must be 0-127");

        SlotNumber = slotNumber;
        Direction = forward;
        F0 = f0;
        F1 = f1;
        F2 = f2;
        F3 = f3;
        F4 = f4;
    }

    /// <summary>
    /// Slot number to control (must be in a consist with SL_CONUP flag).
    /// </summary>
    public byte SlotNumber { get; }

    /// <summary>
    /// Direction: true=forward, false=reverse.
    /// </summary>
    public bool Direction { get; }

    /// <summary>
    /// Function F0 state (typically headlight).
    /// </summary>
    public bool F0 { get; }

    /// <summary>
    /// Function F1 state.
    /// </summary>
    public bool F1 { get; }

    /// <summary>
    /// Function F2 state.
    /// </summary>
    public bool F2 { get; }

    /// <summary>
    /// Function F3 state.
    /// </summary>
    public bool F3 { get; }

    /// <summary>
    /// Function F4 state.
    /// </summary>
    public bool F4 { get; }

    /// <summary>
    /// Creates a consist function command with all functions off and specified direction.
    /// </summary>
    /// <param name="slotNumber">Consist member slot</param>
    /// <param name="forward">Direction</param>
    public static ConsistFunctionCommand DirectionOnly(byte slotNumber, bool forward)
    {
        return new ConsistFunctionCommand(slotNumber, forward, false, false, false, false, false);
    }

    /// <summary>
    /// Creates a consist function command to control only the headlight (F0).
    /// </summary>
    /// <param name="slotNumber">Consist member slot</param>
    /// <param name="forward">Direction</param>
    /// <param name="headlightOn">F0 state</param>
    public static ConsistFunctionCommand Headlight(byte slotNumber, bool forward, bool headlightOn)
    {
        return new ConsistFunctionCommand(slotNumber, forward, headlightOn, false, false, false, false);
    }

    /// <summary>
    /// Generates the 4-byte message: [0xB6, slot, dirf, checksum].
    /// DIRF byte format same as OPC_LOCO_DIRF.
    /// </summary>
    public override byte[] GetBytesWithChecksum()
    {
        // Build DIRF byte (same format as OPC_LOCO_DIRF)
        byte dirf = 0x00;

        if (Direction) dirf |= 0x20;  // Bit 5: Direction
        if (F0) dirf |= 0x10;          // Bit 4: F0
        if (F4) dirf |= 0x08;          // Bit 3: F4
        if (F3) dirf |= 0x04;          // Bit 2: F3
        if (F2) dirf |= 0x02;          // Bit 1: F2
        if (F1) dirf |= 0x01;          // Bit 0: F1

        return AppendChecksum([OperationCode, SlotNumber, dirf]);
    }

    public override string ToString()
    {
        return $"Consist Function Slot {SlotNumber}: {(Direction ? "FWD" : "REV")}, F0={F0}";
    }
}
