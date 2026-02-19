using System.Text.Json.Serialization;

namespace Tellurian.Trains.Communications.Interfaces.Detectors;

/// <summary>
/// Notification for transponder/RailCom events indicating locomotive presence in a section
/// (LocoNet OPC_MULTI_SENSE, Z21 LocoNet detector, Z21 CAN detector).
/// </summary>
public class TransponderNotification(ushort feedbackAddress, ushort locoAddress, bool isEntering, DateTimeOffset timestamp)
    : Notification(timestamp)
{
    [JsonConstructor]
    public TransponderNotification(ushort feedbackAddress, ushort locoAddress, bool isEntering) : this(feedbackAddress, locoAddress, isEntering, DateTimeOffset.Now) { }

    /// <summary>
    /// The feedback module address.
    /// </summary>
    public ushort FeedbackAddress { get; } = feedbackAddress;

    /// <summary>
    /// The locomotive address detected by transponder/RailCom.
    /// </summary>
    public ushort LocoAddress { get; } = locoAddress;

    /// <summary>
    /// True if the locomotive is entering the section, false if leaving.
    /// </summary>
    public bool IsEntering { get; } = isEntering;

    public override string ToString() =>
        $"Transponder {FeedbackAddress}: Loco {LocoAddress} {(IsEntering ? "Entering" : "Leaving")}";
}
