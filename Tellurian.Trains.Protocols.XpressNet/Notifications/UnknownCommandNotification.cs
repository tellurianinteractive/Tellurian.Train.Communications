namespace Tellurian.Trains.Protocols.XpressNet.Notifications;

/// <summary>
/// Sent by Z21 in response to a <see cref="Commands.Command"/> it cannot handle.
/// </summary>
/// <remarks>
/// Reference: Z21 LAN Protokoll Spezifikation 2.11
/// </remarks>
public sealed class UnknownCommandNotification : Notification
{
    internal UnknownCommandNotification() : base(0x61, 0x82) { }
}
