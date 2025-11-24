namespace Tellurian.Trains.Adapters.Z21;

/// <summary>
/// Removes the <see cref="Adapter"/> from the Z21 client list.
/// Log on is implicit when Z21 receives a <see cref="Command"/>.
/// </summary>
/// <remarks>
/// Reference: Z21 LAN Protokoll Spezifikation 2.2
/// </remarks>
public class LogOffCommand : Command
{
    internal override Frame ToFrame()
    {
        return new Frame(FrameHeader.Logoff);
    }
}
