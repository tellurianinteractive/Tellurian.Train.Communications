namespace Tellurian.Trains.Adapters.Z21;

public class GetSubscribedNotificationsCommand : Command
{
    internal override Frame ToFrame()
    {
        return new Frame(FrameHeader.BroadcastSubjects);
    }
}
