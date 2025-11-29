using System.Runtime.Serialization;

namespace Tellurian.Trains.Interfaces.Locos;

[DataContract]
[KnownType(typeof(LocoMovementNotification))]
[KnownType(typeof(LocoFunctionsNotification))]
public abstract class LocoNotification(Address address, DateTimeOffset timestamp) : Notification(timestamp)
{
    [DataMember]
    private readonly Address _Address = address;

    public Address Address => _Address;
    public override bool IsLocoNotification => true;
}
