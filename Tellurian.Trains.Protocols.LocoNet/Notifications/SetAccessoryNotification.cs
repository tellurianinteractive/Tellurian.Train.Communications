using System.Globalization;
using Tellurian.Trains.Communications.Interfaces.Accessories;

namespace Tellurian.Trains.Protocols.LocoNet.Notifications;

/// <summary>
/// OPC_SW_REQ (0xB0) - Accessory set request received as a notification.
/// When another device on the LocoNet bus sends a SetAccessoryCommand,
/// other devices receive it as this notification.
/// </summary>
public sealed class SetAccessoryNotification : Notification
{
    public const byte OperationCode = 0xB0;

    internal SetAccessoryNotification(byte[] data)
    {
        if (data is null || data.Length != 4)
            throw new ArgumentException("Set accessory notification must be exactly 4 bytes", nameof(data));

        ValidateData(OperationCode, data);

        Address = AccessoryAddressExtensions.DecodeAccessoryBytes(data[1], data[2], out var direction, out var output);
        Direction = direction;
        Output = output;
    }

    /// <summary>
    /// The accessory address (1-2048).
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

    public override string ToString()
    {
        return string.Format(CultureInfo.InvariantCulture,
            "SetAccessory {0}: {1} {2}",
            Address.Number,
            Direction == Position.ClosedOrGreen ? "CLOSED" : "THROWN",
            Output == MotorState.On ? "ON" : "OFF");
    }
}
