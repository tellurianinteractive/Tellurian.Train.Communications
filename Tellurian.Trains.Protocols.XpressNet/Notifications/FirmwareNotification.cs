namespace Tellurian.Trains.Protocols.XpressNet.Notifications;

/// <summary>
/// Response from Z21 on a <see cref="Commands.GetFirmwareVersion"/>
/// </summary>
/// <remarks>
/// Reference: Z21 LAN Protokoll Spezifikation 2.15
/// </remarks>
public sealed class FirmwareNotification : Notification
{
    internal FirmwareNotification(byte[] data) : base(0xF3, data) { }

    public byte MajorVersion => Data[2];
    public byte MinorVersion => Data[3];
    public override string ToString() => $"{MajorVersion}.{MinorVersion}";
}
