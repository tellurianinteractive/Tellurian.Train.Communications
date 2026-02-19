namespace Tellurian.Trains.Adapters.Z21;

public static class NotificationFactory
{
    internal static Notification Notification(this Frame sourceFrame)
    {
        return sourceFrame.Header switch
        {
            FrameHeader.BroadcastSubjects => new BroadcastSubjectsNotification(sourceFrame),
            FrameHeader.HardwareInfo => new HardwareInfoNotification(sourceFrame),
            FrameHeader.LocoAddressMode => new LocoAddressModeNotification(sourceFrame),
            FrameHeader.LocoNetReceive => new LocoNetNotification(sourceFrame),
            FrameHeader.LocoNetTransmit => new LocoNetNotification(sourceFrame),
            FrameHeader.LocoNetCommand => new LocoNetNotification(sourceFrame),
            FrameHeader.SerialNumber => new SerialNumberNotification(sourceFrame),
            FrameHeader.SubscribeNotifications => new BroadcastSubjectsNotification(sourceFrame),
            FrameHeader.SystemStateChanged => new SystemStateChangeNotification(sourceFrame),
            FrameHeader.TurnoutAddressMode => new TurnoutAddressModeNotification(sourceFrame),
            FrameHeader.Xbus => new XpressNetNotification(sourceFrame),
            FrameHeader.LocoNetDetector => new LocoNetDetectorNotification(sourceFrame),
            FrameHeader.CanDetector => new CanDetectorNotification(sourceFrame),
            _ => new UnsupportedNotification(sourceFrame),
        };
    }
}
