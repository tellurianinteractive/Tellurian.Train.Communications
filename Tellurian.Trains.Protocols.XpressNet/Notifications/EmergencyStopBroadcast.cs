namespace Tellurian.Trains.Protocols.XpressNet.Notifications;

/// <summary>
/// The command station is in emergency stop mode and has sent emergency stop commands to all
/// locomotives on the track by means of a DCC Broadcast Stop command.The track power remains on, so
/// that turnout control commands can continue to be sent, however no further locomotive commands will be
/// sent to the layout, until the command station is instructed to restart the layout.
/// </summary>
/// <remarks>
/// Reference: Z21 LAN Protokoll Spezifikation 2.14
/// Reference: Lenz XpressNet Specification 2.1.4.3
/// </remarks>
public class EmergencyStopBroadcast : Notification
{
    internal EmergencyStopBroadcast() : base(0x81, 0x00) { }
}
