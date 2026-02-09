using Tellurian.Trains.Communications.Interfaces.Accessories;

namespace Tellurian.Trains.Protocols.LocoNet.Commands;

/// <summary>
/// OPC_SW_ACK (0xBD) - Accessory request with acknowledge.
/// Commands an accessory change and waits for DCS100 acknowledgment.
/// Note: Not supported by DT200 throttles.
/// Response: OPC_LONG_ACK with accept (0x7F) or reject (0x00) code.
/// </summary>
public sealed class AccessoryAcknowledgeCommand : Command
{
    public const byte OperationCode = 0xBD;

    public AccessoryAcknowledgeCommand(Address address, Position direction, MotorState output)
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
    /// Creates a command to throw an accessory with acknowledgment.
    /// </summary>
    /// <param name="address">Accessory address (0-2047)</param>
    /// <param name="activate">True to activate output, false to turn off</param>
    public static AccessoryAcknowledgeCommand Throw(Address address, bool activate = true)
    {
        return new AccessoryAcknowledgeCommand(
            address,
            Position.ThrownOrRed,
            activate ? MotorState.On : MotorState.Off);
    }

    /// <summary>
    /// Creates a command to close an accessory with acknowledgment.
    /// </summary>
    /// <param name="address">Accessory address (0-2047)</param>
    /// <param name="activate">True to activate output, false to turn off</param>
    public static AccessoryAcknowledgeCommand Close(Address address, bool activate = true)
    {
        return new AccessoryAcknowledgeCommand(
            address,
            Position.ClosedOrGreen,
            activate ? MotorState.On : MotorState.Off);
    }

    /// <summary>
    /// Generates the 4-byte message: [0xBD, sw1, sw2, checksum].
    /// Same format as OPC_SW_REQ but expects acknowledgment.
    /// </summary>
    public override byte[] GetBytesWithChecksum()
    {
        var (sw1, sw2) = Address.EncodeAccessoryBytes(Direction, Output);
        return AppendChecksum([OperationCode, sw1, sw2]);
    }
}
