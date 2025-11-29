namespace Tellurian.Trains.Interfaces.Locos;

public abstract class LocoNotification(Address address, DateTimeOffset timestamp) : Notification(timestamp)
{
    private readonly Address _Address = address;

    public Address Address => _Address;
    public override bool IsLocoNotification => true;
}
