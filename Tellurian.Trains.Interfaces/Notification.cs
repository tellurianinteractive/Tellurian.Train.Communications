using System.Runtime.Serialization;

#pragma warning disable CA1051 // Do not declare visible instance fields

namespace Tellurian.Trains.Interfaces;

[DataContract]
[KnownType(typeof(ShortCircuitNotification))]
[KnownType(typeof(MessageNotification))]
[KnownType(typeof(Locos.LocoNotification))]
[KnownType(typeof(Accessories.AccessoryNotification))]
public abstract class Notification(DateTimeOffset timestamp)
{
    protected Notification() : this(DateTimeOffset.Now) { }

    protected Notification(DateTimeOffset timestamp, string message) : this(timestamp) => Message = message ?? string.Empty;

    public virtual bool IsLocoNotification { get; }

    [DataMember]
    public readonly DateTimeOffset Timestamp = timestamp;

    [DataMember]
    public readonly string? Message;

    public override string ToString() => $"{GetType().Name} {Message}";
}
