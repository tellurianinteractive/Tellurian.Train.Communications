using System.Runtime.Serialization;

namespace Tellurian.Trains.Interfaces.Accessories;

/// <summary>
/// Represents a command to control an accessory (switch/turnout/signal).
/// </summary>
[DataContract]
public readonly struct AccessoryCommand : IEquatable<AccessoryCommand>
{
    /// <summary>
    /// Creates a command to close a switch (set to straight position).
    /// </summary>
    /// <param name="activate">True to activate the output; false to just set position.</param>
    public static AccessoryCommand Close(bool activate = true) =>
        new(Position.ClosedOrGreen, activate ? MotorState.On : MotorState.Off);

    /// <summary>
    /// Creates a command to throw a switch (set to diverging position).
    /// </summary>
    /// <param name="activate">True to activate the output; false to just set position.</param>
    public static AccessoryCommand Throw(bool activate = true) =>
        new(Position.ThrownOrRed, activate ? MotorState.On : MotorState.Off);

    /// <summary>
    /// Creates a command to turn off the accessory output.
    /// </summary>
    public static AccessoryCommand TurnOff() =>
        new(Position.ClosedOrGreen, MotorState.Off);

    /// <summary>
    /// Creates a command with the specified function and output state.
    /// </summary>
    public static AccessoryCommand Set(Position function, MotorState output) =>
        new(function, output);

    private AccessoryCommand(Position function, MotorState output)
    {
        _function = (byte)function;
        _output = (byte)output;
    }

    [DataMember(Name = "Function", Order = 1)]
    private readonly byte _function;

    [DataMember(Name = "Output", Order = 2)]
    private readonly byte _output;

    /// <summary>
    /// The function/position to set.
    /// </summary>
    public Position Function => (Position)_function;

    /// <summary>
    /// The output state (on/off).
    /// </summary>
    public MotorState Output => (MotorState)_output;

    /// <summary>
    /// True if the output is activated.
    /// </summary>
    public bool IsActivated => Output == MotorState.On;

    /// <summary>
    /// True if this is a close (straight) command.
    /// </summary>
    public bool IsClose => Function == Position.ClosedOrGreen;

    /// <summary>
    /// True if this is a throw (diverging) command.
    /// </summary>
    public bool IsThrow => Function == Position.ThrownOrRed;

    public bool Equals(AccessoryCommand other) =>
        other.Function == Function && other.Output == Output;

    public override bool Equals(object? obj) =>
        obj is AccessoryCommand other && Equals(other);

    public override int GetHashCode() =>
        HashCode.Combine(Function, Output);

    public override string ToString() =>
        $"{(IsClose ? "Close" : "Throw")} {(IsActivated ? "On" : "Off")}";

    public static bool operator ==(AccessoryCommand left, AccessoryCommand right) =>
        left.Equals(right);

    public static bool operator !=(AccessoryCommand left, AccessoryCommand right) =>
        !(left == right);
}
