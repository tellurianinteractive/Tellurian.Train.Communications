using System.Globalization;


namespace Tellurian.Trains.Protocols.XpressNet.Notifications;

/// <summary>
/// Send by Z21 as a response to a <see cref="Commands.GetVersionCommand"/>.
/// Reference: Z21 LAN Protokoll Spezifikation 2.3
/// Reference: Lenz XpressNet Specification 2.1.6.1 and 2.1.6.2
/// </summary>
public sealed class VersionNotification : Notification
{
    internal VersionNotification(byte[] buffer) : base(GetHeader, buffer, IsExpectedLength) { }

    private static byte GetHeader(byte[] buffer)
    {
        if (buffer[0] != 0x62 && buffer[0] != 0x63) throw new ArgumentOutOfRangeException(nameof(buffer), Resources.Strings.InvalidHeader);
        return buffer[0];
    }

    private static bool IsExpectedLength(byte[] buffer) => buffer.Length >= 4 && buffer.Length <= 5;

    public string Version => string.Format(CultureInfo.InvariantCulture, "{0}.{1}", (Data[1] & 0xF0) / 16, Data[1] & 0x0F);

    public string BusName => Header == 0x62 ? "X-Bus" : "XpressNet";

    public string CommandStationName =>
        Data.Length <= 2 ?
        "Unknown" :
        Data[2] switch
        {
            0x00 => "LZ100",
            0x01 => "LH200",
            0x02 => "DPC",
            0x12 => "Z21",
            _ => "Other",
        };

    public override string ToString() => $"{CommandStationName} {BusName} {Version}";
}
