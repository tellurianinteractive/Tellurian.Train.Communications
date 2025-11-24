using Tellurian.Trains.Interfaces.Extensions;

namespace Tellurian.Trains.Protocols.XpressNet.Notifications;

/// <summary>
/// Response from Z21 on a <see cref="Commands.GetHardwareInfo"/>
/// </summary>
/// <remarks>
/// Reference: Z21 LAN Protokoll Spezifikation 2.20
/// </remarks>
public sealed class HardwareNotification : Notification
{
    internal HardwareNotification(byte[] data) : base(0x1A, data) { }

    private HardwareType Hardware => (HardwareType)Data.ToUint32LittleEndian(2);
    private byte MajorVersion => Data[5];
    private byte MinorVersion => Data[4];

    private enum HardwareType
    {
        Z21Old = 0x00000200,
        Z21New = 0x00000201,
        Smartrail = 0x00000202,
        z21Small = 0x00000203,
        z21Start = 0x00000204
    };

    private string HardwareDescription =>
        Hardware switch
        {
            HardwareType.Smartrail => "Smartrail (2012-)",
            HardwareType.Z21New => "Z21 (2013-)",
            HardwareType.Z21Old => "Z21 (-2012)",
            HardwareType.z21Small => "z21 (2013-)",
            HardwareType.z21Start => "z21 start (2016-)",
            _ => "Uknknown hardware"
        };
    public override string ToString() => $"{HardwareDescription} {MajorVersion}.{MinorVersion}";
}
