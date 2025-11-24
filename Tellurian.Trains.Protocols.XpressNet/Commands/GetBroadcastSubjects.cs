namespace Tellurian.Trains.Protocols.XpressNet.Commands;

/// <summary>
/// Requests Z21 to send a <see cref="Notifications.BroadcastSubjectNotification"/>
/// </summary>
/// <remarks>
/// Reference: Z21 LAN Protokoll Spezifikation 2.17
/// </remarks>
public sealed class GetBroadcastSubjects : Command
{
    public GetBroadcastSubjects() : base(0x51, 0x00) { }
}