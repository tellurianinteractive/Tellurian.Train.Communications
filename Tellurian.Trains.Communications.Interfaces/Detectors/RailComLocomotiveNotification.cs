using System.Text.Json.Serialization;

namespace Tellurian.Trains.Communications.Interfaces.Detectors;

/// <summary>
/// Notification for RailCom/LISSY locomotive identification with direction and classification
/// (Z21 LocoNet detector type 0x10, Z21 CAN detector RailCom addresses).
/// </summary>
public class RailComLocomotiveNotification(ushort feedbackAddress, ushort locoAddress, bool hasDirection, bool isForward, byte classInfo, DateTimeOffset timestamp)
    : Notification(timestamp)
{
    [JsonConstructor]
    public RailComLocomotiveNotification(ushort feedbackAddress, ushort locoAddress, bool hasDirection, bool isForward, byte classInfo)
        : this(feedbackAddress, locoAddress, hasDirection, isForward, classInfo, DateTimeOffset.Now) { }

    /// <summary>
    /// The feedback module address.
    /// </summary>
    public ushort FeedbackAddress { get; } = feedbackAddress;

    /// <summary>
    /// The locomotive address identified by RailCom.
    /// </summary>
    public ushort LocoAddress { get; } = locoAddress;

    /// <summary>
    /// True if direction information is available.
    /// </summary>
    public bool HasDirection { get; } = hasDirection;

    /// <summary>
    /// True if the locomotive is moving forward (only valid when <see cref="HasDirection"/> is true).
    /// </summary>
    public bool IsForward { get; } = isForward;

    /// <summary>
    /// Classification information byte from RailCom/LISSY.
    /// </summary>
    public byte ClassInfo { get; } = classInfo;

    public override string ToString() =>
        $"RailCom {FeedbackAddress}: Loco {LocoAddress}{(HasDirection ? (IsForward ? " Fwd" : " Rev") : "")}";
}
