using Tellurian.Trains.Communications.Interfaces.Extensions;

namespace Tellurian.Trains.Adapters.Z21;

/// <summary>
/// Sent from Z21 as a response on <see cref="GetHardwareInfoCommand"/>.
/// </summary>
/// <remarks>
///  Reference: Z21 LAN Protokoll Spezifikation 2.20
/// </remarks>
public sealed class HardwareInfoNotification : Notification
{
    internal HardwareInfoNotification(Frame frame) : base(frame)
    {
        Name = GetHardwareType(frame.Data);
        Version = $"{Major(frame)}.{Minor(frame)}";
    }
    public string Name { get; }
    public string Version { get; }

    private static string GetHardwareType(byte[] data)
    {
        var value = data.ToInt32LittleEndian();
        return value switch
        {
            0x00000200 => "Z21 old -2012",
            0x00000201 => "Z21 new 2013-",
            0x00000202 => "SmartRail 2012-",
            0x00000203 => "z21 2013-",
            0x00000204 => "z21 smart 2016-",
            _ => "Unknown",
        };
    }
    private static int Major(Frame frame) => frame.Data[5].Bcd();
    private static int Minor(Frame frame) => frame.Data[4].Bcd();
    public override string ToString() => $"Hardware {Name} {Version}";
}
