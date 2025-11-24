namespace Tellurian.Trains.Protocols.XpressNet.Notifications;

/// <summary>
/// Base class for all programming mode notifications.
/// </summary>
public abstract class ProgrammingModeBroadcast : Notification
{
    protected ProgrammingModeBroadcast(byte header, byte data) : base(header, data) { }
}

///<summary>
///Until the Command Station has exited programming mode, no further communication will take place
///with any XpressNet device other than the XpressNet device that requested the system to enter
///service mode.
///</summary>
/// <remarks>
/// Reference: Z21 LAN Protokoll Spezifikation 2.9
/// Reference: Lenz XpressNet Specification 2.1.4.4
/// </remarks>
public sealed class ProgrammingModeEnteredBroadcast : ProgrammingModeBroadcast
{
    internal ProgrammingModeEnteredBroadcast() : base(0x61, 0x02) { }
}

/// <summary>
/// This response is sent if a short circuit (too high a current draw) is detected on entry to service mode. It
/// should be assumed that a write instruction sent to the decoder was not successful.Upon receipt of
/// this instruction subsequent service mode requests should not be sent until the user has corrected the
/// problem.
/// </summary>
/// <remarks>
/// Reference: Lenz XpressNet Specification 2.1.5.1
/// </remarks>
public sealed class ProgrammingTrackShortCircuitBroadcast : ProgrammingModeBroadcast
{
    internal ProgrammingTrackShortCircuitBroadcast() : base(0x60, 0x12) { }
}

/// <summary>
/// A service mode read request resulted in no acknowledgement (timed out). Programming of this decoder should be
/// cancelled or tried again.
/// Also called "Data byte not found", meanining request for CV-value did not respond timely.
/// </summary>
/// <remarks>
/// Reference: Lenz XpressNet Specification 2.1.5.2
/// </remarks>
public sealed class ProgrammingCommandNotAcknowledgedBroadcast : ProgrammingModeBroadcast
{
    internal ProgrammingCommandNotAcknowledgedBroadcast() : base(0x60, 0x13) { }
}

/// <summary>
///
/// </summary>
/// <remarks>
/// Reference: Lenz XpressNet Specification 2.1.5.3
/// </remarks>
public sealed class ProgrammingStationBusyBroadcast : ProgrammingModeBroadcast
{
    internal ProgrammingStationBusyBroadcast() : base(0x60, 0x1F) { }
}

/// <summary>
///
/// </summary>
/// <remarks>
/// Reference: Lenz XpressNet Specification 2.1.5.4
/// </remarks>
public sealed class ProgrammingStationReadyBroadcast : ProgrammingModeBroadcast
{
    internal ProgrammingStationReadyBroadcast() : base(0x60, 0x11) { }
}
