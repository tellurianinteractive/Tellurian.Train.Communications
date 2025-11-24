namespace Tellurian.Trains.Protocols.XpressNet.Commands;

/// <summary>
/// Requests Z21 to poewr off tracks and reply with a <see cref="Notifications.TrackPowerOnBroadcast"/>
/// </summary>
/// <remarks>
/// Reference: Z21 LAN Protokoll Spezifikation 2.6
/// </remarks>
public sealed class TrackPowerOnCommand : Command
{
    public TrackPowerOnCommand() : base(0x21, 0x81) { }
}
