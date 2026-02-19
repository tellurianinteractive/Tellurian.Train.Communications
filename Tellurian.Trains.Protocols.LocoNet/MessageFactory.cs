using Tellurian.Trains.Protocols.LocoNet.Commands;
using Tellurian.Trains.Protocols.LocoNet.Notifications;

namespace Tellurian.Trains.Protocols.LocoNet;

/// <summary>
/// Factory for creating LocoNet message objects from byte arrays.
/// </summary>
public static class LocoNetMessageFactory
{
    /// <summary>
    /// Creates the appropriate Message-derived object from raw LocoNet message bytes.
    /// </summary>
    /// <param name="data">Complete LocoNet message including opcode and checksum</param>
    /// <returns>Parsed message object (Command or Notification)</returns>
    /// <exception cref="ArgumentNullException">If data is null or empty</exception>
    public static Message Create(byte[]? data)
    {
        if (data == null || data.Length == 0)
            throw new ArgumentNullException(nameof(data));

        var opcode = data[0];

        return opcode switch
        {
            MasterBusyNotification.OperationCode => new MasterBusyNotification(),
            PowerOffCommand.OperationCode => new PowerOffCommand(),
            PowerOnCommand.OperationCode => new PowerOnCommand(),
            ForceIdleCommand.OperationCode => new ForceIdleCommand(),
            SetAccessoryNotification.OperationCode => new SetAccessoryNotification(data),
            AccessoryReportNotification.OperationCode => new AccessoryReportNotification(data),
            SensorInputNotification.OperationCode => new SensorInputNotification(data),
            MultiSenseNotification.OperationCode => new MultiSenseNotification(data),
            LongAcknowledge.OperationCode => new LongAcknowledge(data),
            SlotNotification.OperationCode => new SlotNotification(data),
            LissyNotification.OperationCode when LissyNotification.IsLissyMessage(data) => new LissyNotification(data),
            LissyNotification.OperationCode => new UnsupportedNotification(data),
            LncvNotification.OperationCode when LncvNotification.IsLncvMessage(data) => new LncvNotification(data),
            LncvNotification.OperationCode => new UnsupportedNotification(data),
            _ => new UnsupportedNotification(data)
        };
    }
}

