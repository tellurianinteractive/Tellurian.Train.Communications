namespace Tellurian.Trains.Protocols.XpressNet.Commands;

/// <summary>
/// Requests Z21 to poewr off tracks and reply with a <see cref="Notifications.TrackPowerOffBroadcast"/>
/// </summary>
/// <remarks>
/// Reference: Z21 LAN Protokoll Spezifikation 2.5
/// </remarks>
public sealed class TrackPowerOffCommand : Command
{
    public TrackPowerOffCommand() : base(0x21, 0x80) { }
}
