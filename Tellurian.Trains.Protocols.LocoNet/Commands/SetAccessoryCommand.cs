using Tellurian.Trains.Communications.Interfaces.Accessories;

namespace Tellurian.Trains.Protocols.LocoNet.Commands;

/// <summary>
/// OPC_SW_REQ (0xB0) - Accessory request without acknowledge.
/// Commands an accessory change without waiting for acknowledgment.
/// This is the most common accessory control command.
/// No response is sent by default.
/// </summary>
public sealed class SetAccessoryCommand : Command
{
    public const byte OperationCode = 0xB0;

    public SetAccessoryCommand(Address address, Position direction, MotorState output)
    {
        Address = address;
        Direction = direction;
        Output = output;
    }

    /// <summary>
    /// The accessory address (0-2047).
    /// </summary>
    public Address Address { get; }

    /// <summary>
    /// Direction/function: Closed/Green or Thrown/Red.
    /// </summary>
    public Position Direction { get; }

    /// <summary>
    /// Output state: On or Off.
    /// </summary>
    public MotorState Output { get; }

    /// <summary>
    /// Creates a command to throw an accessory (set to Thrown/Red position).
    /// </summary>
    /// <param name="address">Accessory address (0-2047)</param>
    /// <param name="activate">True to activate output, false to turn off</param>
    public static SetAccessoryCommand Throw(Address address, bool activate = true)
    {
        return new SetAccessoryCommand(
            address,
            Position.ThrownOrRed,
            activate ? MotorState.On : MotorState.Off);
    }

    /// <summary>
    /// Creates a command to close an accessory (set to Closed/Green position).
    /// </summary>
    /// <param name="address">Accessory address (0-2047)</param>
    /// <param name="activate">True to activate output, false to turn off</param>
    public static SetAccessoryCommand Close(Address address, bool activate = true)
    {
        return new SetAccessoryCommand(
            address,
            Position.ClosedOrGreen,
            activate ? MotorState.On : MotorState.Off);
    }

    /// <summary>
    /// Creates a command to turn off an accessory output (typically sent after activation).
    /// This prevents motor overheating in turnout motors.
    /// </summary>
    /// <param name="address">Accessory address (0-2047)</param>
    public static SetAccessoryCommand TurnOff(Address address)
    {
        return new SetAccessoryCommand(
            address,
            Position.ClosedOrGreen,
            MotorState.Off);
    }

    /// <summary>
    /// Generates the 4-byte message: [0xB0, sw1, sw2, checksum].
    /// </summary>
    public override byte[] GetBytesWithChecksum()
    {
        var (sw1, sw2) = Address.EncodeAccessoryBytes(Direction, Output);
        return AppendChecksum([OperationCode, sw1, sw2]);
    }
}
