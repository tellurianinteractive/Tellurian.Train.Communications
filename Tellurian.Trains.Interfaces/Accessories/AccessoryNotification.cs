using System.Runtime.Serialization;

namespace Tellurian.Trains.Interfaces.Accessories;

/// <summary>
/// Notification for accessory state changes.
/// </summary>
[DataContract]
public class AccessoryNotification(Address address, Position function, DateTimeOffset timestamp)
    : Notification(timestamp)
{
    [DataMember]
    private readonly Address _Address = address;

    [DataMember]
    private readonly Position _Function = function;

    /// <summary>
    /// The address of the accessory.
    /// </summary>
    public Address Address => _Address;

    /// <summary>
    /// The current function/position of the accessory.
    /// </summary>
    public Position Function => _Function;

    /// <summary>
    /// True if the accessory is in closed (straight) position.
    /// </summary>
    public bool IsClosed => Function == Position.ClosedOrGreen;

    /// <summary>
    /// True if the accessory is in thrown (diverging) position.
    /// </summary>
    public bool IsThrown => Function == Position.ThrownOrRed;

    public override string ToString() =>
        $"Accessory {Address}: {(IsClosed ? "Closed" : "Thrown")}";
}
