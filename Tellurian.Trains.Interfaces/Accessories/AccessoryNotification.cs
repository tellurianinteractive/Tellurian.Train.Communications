using System.Text.Json.Serialization;

namespace Tellurian.Trains.Interfaces.Accessories;

/// <summary>
/// Notification for accessory state changes.
/// </summary>
public class AccessoryNotification(Address address, Position function, DateTimeOffset timestamp)
    : Notification(timestamp)
{
    [JsonConstructor]
    public AccessoryNotification(Address address, Position function) : this(address, function, DateTimeOffset.Now) { }

    /// <summary>
    /// The address of the accessory.
    /// </summary>
    public Address Address { get; } = address;

    /// <summary>
    /// The current function/position of the accessory.
    /// </summary>
    public Position Function { get; } = function;

    /// <summary>
    /// True if the accessory is in closed (straight) position.
    /// </summary>
    [JsonIgnore]
    public bool IsClosed => Function == Position.ClosedOrGreen;

    /// <summary>
    /// True if the accessory is in thrown (diverging) position.
    /// </summary>
    [JsonIgnore]
    public bool IsThrown => Function == Position.ThrownOrRed;

    public override string ToString() =>
        $"Accessory {Address}: {(IsClosed ? "Closed" : "Thrown")}";
}
