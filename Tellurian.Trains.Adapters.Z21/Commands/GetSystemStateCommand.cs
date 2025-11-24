namespace Tellurian.Trains.Adapters.Z21;

/// <summary>
/// Requests Z21 to send <see cref="SystemStateChangeNotification"/>
/// </summary>
public class GetSystemStateCommand : Command
{
    internal override Frame ToFrame()
    {
        return new Frame(FrameHeader.SystemState);
    }
}
