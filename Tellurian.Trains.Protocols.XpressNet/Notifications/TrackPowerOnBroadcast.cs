namespace Tellurian.Trains.Protocols.XpressNet.Notifications;

/// <summary>
/// Sent by Z21 when track power is turned on,
/// for example in response on a <see cref="Commands.TrackPowerOnCommand"/>.
/// </summary>
/// <remarks>
/// Reference: Z21 LAN Protokoll Spezifikation 2.8
/// </remarks>
public class TrackPowerOnBroadcast : Notification
{
    internal TrackPowerOnBroadcast() : base(0x60, 0x01) { }
}
