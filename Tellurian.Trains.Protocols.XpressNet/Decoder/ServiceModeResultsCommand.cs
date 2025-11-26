namespace Tellurian.Trains.Protocols.XpressNet.Decoder;

/// <summary>
/// Request for Service Mode results (spec section 4.10).
/// Requests the command station to transmit back the result of a preceding read action.
/// </summary>
/// <remarks>
/// Format: Header=0x21, Data=[0x10]
///
/// This command must be sent after a service mode read command to retrieve the result.
/// The command station will respond with one of:
/// - ServiceModeRegisterPagedNotification (0x63, 0x10) for Register/Paged mode reads
/// - ServiceModeDirectCVNotification (0x63, 0x14) for Direct CV mode reads
/// - ProgrammingTrackShortCircuitBroadcast (0x61, 0x12) if short circuit detected
/// - ProgrammingCommandNotAcknowledgedBroadcast (0x61, 0x13) if no acknowledgement received
/// - ProgrammingStationBusyBroadcast (0x61, 0x1F) if command station is busy
/// - ProgrammingStationReadyBroadcast (0x61, 0x11) if command station is ready
/// </remarks>
public sealed class ServiceModeResultsCommand : Commands.Command
{
    public ServiceModeResultsCommand() : base(0x21, 0x10) { }
}
