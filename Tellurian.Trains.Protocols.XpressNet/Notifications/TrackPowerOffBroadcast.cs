namespace Tellurian.Trains.Protocols.XpressNet.Notifications;

/// <summary>
/// Sent by command station when the DCC track power is switched off,
/// for example in response on a <see cref="Commands.TrackPowerOffCommand"/>.
/// No additional commands can be sent to the track until normal operations are resumed.
/// While track power is off, state requests can still be sent to the command station.
/// This can be used to change the state of the operations once normal operations are resumed.
/// </summary>
/// <remarks>
/// Reference: Lenz XpressNet Specification 2.1.4.2
/// Reference: Z21 LAN Protokoll Spezifikation 2.7
/// </remarks>
public class TrackPowerOffBroadcast : Notification
{
    internal TrackPowerOffBroadcast() : base(0x60, 0x00) { }
}
