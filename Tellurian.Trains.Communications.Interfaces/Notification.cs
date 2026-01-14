using System.Text.Json.Serialization;
using Tellurian.Trains.Communications.Interfaces.Accessories;
using Tellurian.Trains.Communications.Interfaces.Locos;

namespace Tellurian.Trains.Communications.Interfaces;

/// <summary>
/// Base class for all protocol-agnostic notifications.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(MessageNotification), "MessageNotification")]
[JsonDerivedType(typeof(ShortCircuitNotification), "ShortCircuitNotification")]
[JsonDerivedType(typeof(LocoMovementNotification), "LocoMovementNotification")]
[JsonDerivedType(typeof(LocoFunctionsNotification), "LocoFunctionsNotification")]
[JsonDerivedType(typeof(AccessoryNotification), "AccessoryNotification")]
public abstract class Notification(DateTimeOffset timestamp)
{
    protected Notification() : this(DateTimeOffset.Now) { }

    protected Notification(DateTimeOffset timestamp, string message) : this(timestamp) => Message = message ?? string.Empty;

    [JsonIgnore]
    public virtual bool IsLocoNotification { get; }

    public DateTimeOffset Timestamp { get; } = timestamp;

    public string? Message { get; init; }

    public override string ToString() => $"{GetType().Name} {Message}";
}
