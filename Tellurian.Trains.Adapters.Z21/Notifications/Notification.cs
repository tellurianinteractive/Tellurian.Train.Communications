namespace Tellurian.Trains.Adapters.Z21;

public abstract class Notification : Message
{
    protected Notification(Frame frame) : base(frame) { }
}
