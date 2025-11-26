namespace Tellurian.Trains.Protocols.LocoNet.Commands;

/// <summary>
/// OPC_SW_REQ (0xB0) - Switch request without acknowledge.
/// Commands a turnout/switch change without waiting for acknowledgment.
/// This is the most common switch control command.
/// No response is sent by default.
/// </summary>
public sealed class SetTurnoutCommand : Command
{
    public const byte OperationCode = 0xB0;

    public SetTurnoutCommand(AccessoryAddress address, AccessoryFunction direction, OutputState output)
    {
        Address = address;
        Direction = direction;
        Output = output;
    }

    /// <summary>
    /// The accessory address (0-2047).
    /// </summary>
    public AccessoryAddress Address { get; }

    /// <summary>
    /// Direction/function: Closed/Green or Thrown/Red.
    /// </summary>
    public AccessoryFunction Direction { get; }

    /// <summary>
    /// Output state: On or Off.
    /// </summary>
    public OutputState Output { get; }

    /// <summary>
    /// Creates a command to throw a switch (set to Thrown/Red position).
    /// </summary>
    /// <param name="address">Switch address (0-2047)</param>
    /// <param name="activate">True to activate output, false to turn off</param>
    public static SetTurnoutCommand Throw(AccessoryAddress address, bool activate = true)
    {
        return new SetTurnoutCommand(
            address,
            AccessoryFunction.ThrownOrRed,
            activate ? OutputState.On : OutputState.Off);
    }

    /// <summary>
    /// Creates a command to close a switch (set to Closed/Green position).
    /// </summary>
    /// <param name="address">Switch address (0-2047)</param>
    /// <param name="activate">True to activate output, false to turn off</param>
    public static SetTurnoutCommand Close(AccessoryAddress address, bool activate = true)
    {
        return new SetTurnoutCommand(
            address,
            AccessoryFunction.ClosedOrGreen,
            activate ? OutputState.On : OutputState.Off);
    }

    /// <summary>
    /// Creates a command to turn off a switch output (typically sent after activation).
    /// This prevents motor overheating in turnout motors.
    /// </summary>
    /// <param name="address">Switch address (0-2047)</param>
    public static SetTurnoutCommand TurnOff(AccessoryAddress address)
    {
        return new SetTurnoutCommand(
            address,
            AccessoryFunction.ClosedOrGreen,
            OutputState.Off);
    }

    /// <summary>
    /// Generates the 4-byte message: [0xB0, sw1, sw2, checksum].
    /// </summary>
    public override byte[] GetBytesWithChecksum()
    {
        var (sw1, sw2) = Address.EncodeSwitchBytes(Direction, Output);
        return AppendChecksum([OperationCode, sw1, sw2]);
    }
}
