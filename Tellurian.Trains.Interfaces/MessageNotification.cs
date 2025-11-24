using System.Runtime.Serialization;

namespace Tellurian.Trains.Interfaces;

[DataContract]
public sealed class MessageNotification : Notification
{
    public MessageNotification(DateTimeOffset timestamp, string message) : base(timestamp, message) { }
    public MessageNotification() : base(DateTimeOffset.Now) { }
    public override string ToString() => Message ?? nameof(MessageNotification);
}
