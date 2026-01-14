using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tellurian.Trains.Communications.Interfaces.Accessories;

/// <summary>
/// Represents the output state of an accessory.
/// </summary>
[JsonConverter(typeof(MotorStateConverter))]
public enum MotorState : byte
{
    /// <summary>
    /// Output is off (motor/coil deactivated).
    /// </summary>
    Off = 0,
    /// <summary>
    /// Output is on (motor/coil activated).
    /// </summary>
    On = 1
}

internal sealed class MotorStateConverter() : JsonStringEnumConverter<MotorState>(JsonNamingPolicy.CamelCase);
