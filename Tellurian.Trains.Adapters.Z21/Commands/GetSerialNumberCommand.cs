namespace Tellurian.Trains.Adapters.Z21;

/// <summary>
/// Requests Z21 to send <see cref="SerialNumberNotification"/>
/// </summary>
/// <remarks>
/// Reference: Z21 LAN Protokoll Spezifikation 2.1
/// </remarks>
public class GetSerialNumberCommand : Command
{
    internal override Frame ToFrame()
    {
        return new Frame(FrameHeader.SerialNumber);
    }
}
