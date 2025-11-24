using System.Runtime.Serialization;

namespace Tellurian.Trains.Interfaces;

[DataContract]
public sealed class ShortCircuitNotification : Notification
{
    public ShortCircuitNotification(DateTimeOffset timestamp) : base(timestamp) { }
    public ShortCircuitNotification() : base(DateTimeOffset.Now) { }
}
