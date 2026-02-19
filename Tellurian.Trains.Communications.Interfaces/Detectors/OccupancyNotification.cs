using System.Text.Json.Serialization;

namespace Tellurian.Trains.Communications.Interfaces.Detectors;

/// <summary>
/// Notification for occupancy state changes from any source
/// (LocoNet OPC_INPUT_REP, Z21 LocoNet detector, Z21 CAN detector).
/// </summary>
public class OccupancyNotification(ushort feedbackAddress, bool isOccupied, DateTimeOffset timestamp)
    : Notification(timestamp)
{
    [JsonConstructor]
    public OccupancyNotification(ushort feedbackAddress, bool isOccupied) : this(feedbackAddress, isOccupied, DateTimeOffset.Now) { }

    /// <summary>
    /// The feedback module address.
    /// </summary>
    public ushort FeedbackAddress { get; } = feedbackAddress;

    /// <summary>
    /// True if the section is occupied, false if free.
    /// </summary>
    public bool IsOccupied { get; } = isOccupied;

    public override string ToString() =>
        $"Occupancy {FeedbackAddress}: {(IsOccupied ? "Occupied" : "Free")}";
}
