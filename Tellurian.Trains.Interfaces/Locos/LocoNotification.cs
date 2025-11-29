namespace Tellurian.Trains.Interfaces.Locos;

public abstract class LocoNotification(Address address, DateTimeOffset timestamp) : Notification(timestamp)
{
    public Address Address { get; } = address;
    public override bool IsLocoNotification => true;
}
