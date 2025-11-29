namespace Tellurian.Trains.Interfaces;

public sealed class MessageNotification : Notification
{
    public MessageNotification(DateTimeOffset timestamp, string message) : base(timestamp, message) { }
    public MessageNotification() : base(DateTimeOffset.Now) { }
    public override string ToString() => Message ?? nameof(MessageNotification);
}
