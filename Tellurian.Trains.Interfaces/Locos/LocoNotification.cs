using System.Runtime.Serialization;

namespace Tellurian.Trains.Interfaces.Locos;

[DataContract]
[KnownType(typeof(MovementLocoNotification))]
[KnownType(typeof(FunctionsLocoNotification))]
public abstract class LocoNotification(LocoAddress address, DateTimeOffset timestamp) : Notification(timestamp)
{
    [DataMember]
    private readonly LocoAddress _Address = address;

    public LocoAddress Address => _Address;
    public override bool IsLocoNotification => true;
}
