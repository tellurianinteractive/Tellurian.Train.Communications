namespace Tellurian.Trains.Interfaces;

public sealed class ShortCircuitNotification : Notification
{
    public ShortCircuitNotification(DateTimeOffset timestamp) : base(timestamp) { }
    public ShortCircuitNotification() : base(DateTimeOffset.Now) { }
}
