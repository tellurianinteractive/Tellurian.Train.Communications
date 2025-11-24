namespace Tellurian.Trains.Protocols.XpressNet.Commands;

/// <summary>
/// Requests Z21 to send a <see cref="Notifications.SystemStateChangedNotification"/>
/// </summary>
/// <remarks>
/// Reference: Z21 LAN Protokoll Spezifikation 2.19
/// </remarks>
public sealed class GetSystemStatusCommand : Command
{
    public GetSystemStatusCommand() : base(0x85, 0x00) { }
}
