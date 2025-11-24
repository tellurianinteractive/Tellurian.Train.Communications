namespace Tellurian.Trains.Protocols.XpressNet.Commands;

/// <summary>
/// Requests Z21 to send a <see cref="Notifications.StatusChangedNotification"/>
/// </summary>
/// <remarks>
/// Reference: Z21 LAN Protokoll Spezifikation 2.4
/// </remarks>
public sealed class GetStatusCommand : Command
{
    public GetStatusCommand() : base(0x21, 0x24) {}
}
