namespace Tellurian.Trains.Adapters.Z21;

/// <summary>
/// Wraps data for unsupported messages from Z21.
/// </summary>
public sealed class UnsupportedNotification : Notification
{
    internal UnsupportedNotification(Frame frame) : base(frame)
    {
        Frame = frame;
    }
    private readonly Frame Frame;
    public override string ToString() => BitConverter.ToString(Frame.GetBytes());
}
