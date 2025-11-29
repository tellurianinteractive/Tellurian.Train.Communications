using System.Runtime.Serialization;

namespace Tellurian.Trains.Interfaces.Accessories;

/// <summary>
/// Represents the output state of an accessory.
/// </summary>
[DataContract]
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
