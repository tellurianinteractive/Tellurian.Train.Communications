namespace Tellurian.Trains.Protocols.LocoNet;

/// <summary>
/// Slot busy/active status (bits 5-4 of STAT1 byte).
/// </summary>
public enum SlotStatus : byte
{
    /// <summary>
    /// Empty slot, no locomotive assigned.
    /// </summary>
    Free = 0b00,

    /// <summary>
    /// Loco address assigned, being refreshed, but not controlled by anyone.
    /// </summary>
    Common = 0b01,

    /// <summary>
    /// Loco address assigned, NOT being refreshed (idle).
    /// </summary>
    Idle = 0b10,

    /// <summary>
    /// Loco address assigned, being refreshed and actively controlled.
    /// </summary>
    InUse = 0b11
}

/// <summary>
/// Decoder type/speed step mode (bits 2-0 of STAT1 byte).
/// </summary>
public enum DecoderType : byte
{
    /// <summary>
    /// 28-step mode, 3-byte packet, regular mode.
    /// </summary>
    Steps28 = 0b000,

    /// <summary>
    /// 28-step mode with trinary packets.
    /// </summary>
    Steps28Trinary = 0b001,

    /// <summary>
    /// 14-step mode.
    /// </summary>
    Steps14 = 0b010,

    /// <summary>
    /// 128-step mode (most common).
    /// </summary>
    Steps128 = 0b011,

    /// <summary>
    /// 28-step mode with advanced DCC consisting.
    /// </summary>
    Steps28AdvancedConsist = 0b100,

    /// <summary>
    /// 128-step mode with advanced DCC consisting.
    /// </summary>
    Steps128AdvancedConsist = 0b111
}

/// <summary>
/// Global track status flags (TRK byte in slot data).
/// </summary>
[Flags]
public enum TrackStatus : byte
{
    /// <summary>
    /// No flags set.
    /// </summary>
    None = 0,

    /// <summary>
    /// Track power is ON (bit 0).
    /// </summary>
    PowerOn = 0b0001,

    /// <summary>
    /// Track is not idle/paused (bit 1). 0 = track paused/emergency stop.
    /// </summary>
    NotIdle = 0b0010,

    /// <summary>
    /// LocoNet 1.1 mode (bit 2). 0 = DT200 mode.
    /// </summary>
    LocoNet11 = 0b0100,

    /// <summary>
    /// Programming track is busy (bit 3).
    /// </summary>
    ProgrammingBusy = 0b1000
}
