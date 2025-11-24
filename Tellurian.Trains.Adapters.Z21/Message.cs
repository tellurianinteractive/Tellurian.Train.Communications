namespace Tellurian.Trains.Adapters.Z21;

public abstract class Message
{
    protected Message()
    {
        Timestamp = DateTimeOffset.Now;
    }

    protected Message(Frame frame)
    {
        if (frame is null) throw new ArgumentNullException(nameof(frame));
        Timestamp = frame.Timestamp;
    }

    public DateTimeOffset Timestamp { get; private set; }
    public override string ToString() => $"{GetType().Name}";
}
