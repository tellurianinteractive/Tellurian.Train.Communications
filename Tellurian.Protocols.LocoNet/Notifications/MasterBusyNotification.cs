namespace Tellurian.Trains.Protocols.LocoNet.Notifications;

public sealed class MasterBusyNotification : Notification
{
    public const byte OperationCode = 0x81;

    internal MasterBusyNotification() { }
}
