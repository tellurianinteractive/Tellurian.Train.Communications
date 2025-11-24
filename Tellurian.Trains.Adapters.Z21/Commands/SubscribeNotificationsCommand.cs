namespace Tellurian.Trains.Adapters.Z21;

/// <summary>
/// Updates the clients subscription in Z21.
/// </summary>
public class SubscribeNotificationsCommand : Command
{
    private readonly BroadcastSubjects Subjects;
    public SubscribeNotificationsCommand(BroadcastSubjects subjects)
    {
        Subjects = subjects;
    }

    internal override Frame ToFrame()
    {
        return new Frame(FrameHeader.SubscribeNotifications, BitConverter.GetBytes((Int32)Subjects));
    }
}
