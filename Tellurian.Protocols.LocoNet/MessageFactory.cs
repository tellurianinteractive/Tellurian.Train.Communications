using Tellurian.Trains.Protocols.LocoNet.Commands;
using Tellurian.Trains.Protocols.LocoNet.Notifications;

namespace Tellurian.Trains.Protocols.LocoNet;
public static class LocoNetMessageFactory
{
    public static Message Create(byte[]? data)
    {
        if (data == null || data.Length == 0) throw new ArgumentNullException(nameof(data));
        var opcode = data[0];
        return opcode switch
        {
            MasterBusyNotification.OperationCode => new MasterBusyNotification(),
            PowerOffCommand.OperationCode => new PowerOffCommand(),
            PowerOnCommand.OperationCode => new PowerOnCommand(),
            ForceIdleCommand.OperationCode => new ForceIdleCommand(),
            
            _ => new UnsupportedNotification(data)
        };
    }
}
