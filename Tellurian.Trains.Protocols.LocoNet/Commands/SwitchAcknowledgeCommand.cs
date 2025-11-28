using Tellurian.Trains.Interfaces.Accessories;

namespace Tellurian.Trains.Protocols.LocoNet.Commands;

/// <summary>
/// OPC_SW_ACK (0xBD) - Switch request with acknowledge.
/// Commands a turnout/switch change and waits for DCS100 acknowledgment.
/// Note: Not supported by DT200 throttles.
/// Response: OPC_LONG_ACK with accept (0x7F) or reject (0x00) code.
/// </summary>
public sealed class SwitchAcknowledgeCommand : Command
{
    public const byte OperationCode = 0xBD;

    public SwitchAcknowledgeCommand(AccessoryAddress address, AccessoryFunction direction, OutputState output)
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
    /// Creates a command to throw a switch with acknowledgment.
    /// </summary>
    /// <param name="address">Switch address (0-2047)</param>
    /// <param name="activate">True to activate output, false to turn off</param>
    public static SwitchAcknowledgeCommand Throw(AccessoryAddress address, bool activate = true)
    {
        return new SwitchAcknowledgeCommand(
            address,
            AccessoryFunction.ThrownOrRed,
            activate ? OutputState.On : OutputState.Off);
    }

    /// <summary>
    /// Creates a command to close a switch with acknowledgment.
    /// </summary>
    /// <param name="address">Switch address (0-2047)</param>
    /// <param name="activate">True to activate output, false to turn off</param>
    public static SwitchAcknowledgeCommand Close(AccessoryAddress address, bool activate = true)
    {
        return new SwitchAcknowledgeCommand(
            address,
            AccessoryFunction.ClosedOrGreen,
            activate ? OutputState.On : OutputState.Off);
    }

    /// <summary>
    /// Generates the 4-byte message: [0xBD, sw1, sw2, checksum].
    /// Same format as OPC_SW_REQ but expects acknowledgment.
    /// </summary>
    public override byte[] GetBytesWithChecksum()
    {
        var (sw1, sw2) = Address.EncodeSwitchBytes(Direction, Output);
        return AppendChecksum([OperationCode, sw1, sw2]);
    }
}
