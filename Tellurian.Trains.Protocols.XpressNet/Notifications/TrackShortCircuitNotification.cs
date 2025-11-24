namespace Tellurian.Trains.Protocols.XpressNet.Notifications;

/// <summary>
/// Sent by Z21 when a track short circuit occurs.
/// </summary>
/// <remarks>
/// Reference: Z21 LAN Protokoll Spezifikation 2.10
/// </remarks>
public sealed class TrackShortCircuitNotification : Notification
{
    internal TrackShortCircuitNotification() : base(0x61, 0x08) { }
}
