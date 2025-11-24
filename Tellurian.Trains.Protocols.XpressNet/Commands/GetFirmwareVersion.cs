namespace Tellurian.Trains.Protocols.XpressNet.Commands;

/// <summary>
/// Requests Z21 to send a <see cref="Notifications.FirmwareNotification"/>
/// </summary>
/// <remarks>
/// Reference: Z21 LAN Protokoll Spezifikation 2.15
/// </remarks>
public sealed class GetFirmwareVersion : Command
{
    public GetFirmwareVersion() : base(0xF1, 0x0A) { }
}