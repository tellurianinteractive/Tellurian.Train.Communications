using System.Text.Json.Serialization;

namespace Tellurian.Trains.Interfaces;

public sealed class ShortCircuitNotification : Notification
{
    [JsonConstructor]
    public ShortCircuitNotification(DateTimeOffset timestamp) : base(timestamp) { }
    public ShortCircuitNotification() : base(DateTimeOffset.Now) { }
}
