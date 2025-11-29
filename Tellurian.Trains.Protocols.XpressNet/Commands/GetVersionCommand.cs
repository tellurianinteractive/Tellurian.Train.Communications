namespace Tellurian.Trains.Protocols.XpressNet.Commands;

/// <summary>
/// Requests Z21 to send a <see cref="Notifications.VersionNotification"/>
/// </summary>
/// <remarks>
/// Reference: Z21 LAN Protokoll Spezifikation 2.3
/// </remarks>
public sealed class GetVersionCommand : Command
{
    public GetVersionCommand() : base(0x21, 0x21) { }
}