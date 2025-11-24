namespace Tellurian.Trains.Adapters.Z21;

/// <summary>
/// Reqests Z21 for a <see cref="HardwareInfoNotification"/>.
/// </summary>
/// <remarks>
///  Reference: Z21 LAN Protokoll Spezifikation 2.20
/// </remarks>
public class GetHardwareInfoCommand : Command
{
    internal override Frame ToFrame()
    {
        return new Frame(FrameHeader.HardwareInfo);
    }
}
