using Tellurian.Trains.Protocols.XpressNet.Decoder;

namespace Tellurian.Trains.Protocols.XpressNet.Notifications;

public static class NotificationFactory
{
    public const string SourceBusName = "XpressNet";

    public static Notification Create(byte[] buffer) {
        if (buffer == null) throw new ArgumentNullException(nameof(buffer));
        var Xheader = buffer[0]; // & 0xF0;
        byte db0 = (buffer.Length > 1 ? buffer[1] : (byte)0x00);
        return Xheader switch
        {
            //0x40 => new FeedbackBroadcast(buffer),
            //0x43 TurnoutInfoFeedback
            0x50 => db0 switch
            {
                0x00 => new BroadcastSubjectNotification(buffer),
                _ => new NotSupportedNotification(buffer, SourceBusName)
            },
            0x60 => db0 switch
            {
                0x12 => new ProgrammingTrackShortCircuitBroadcast(),
                0x13 => new ProgrammingCommandNotAcknowledgedBroadcast(),
                0x17 => new ProgrammingStationReadyBroadcast(),
                0x1F => new ProgrammingStationBusyBroadcast(),
                _ => new NotSupportedNotification(buffer, SourceBusName)
            },
            0x61 => db0 switch
            {
                0x00 => new TrackPowerOffBroadcast(),
                0x01 => new TrackPowerOnBroadcast(),
                0x02 => new ProgrammingModeEnteredBroadcast(),
                0x08 => new TrackShortCircuitNotification(),
                0x11 => new ProgrammingStationReadyBroadcast(),
                0x12 => new WriteCVShortCircuitResponse(),
                0x13 => new WriteCVTimeoutResponse(),
                0x14 => new CVOkResponse(buffer),
                0x1F => new ProgrammingStationBusyBroadcast(),
                0x82 => new UnknownCommandNotification(),
                _ => new NotSupportedNotification(buffer, SourceBusName)
            },
            0x62 => db0 switch
            {
                0x22 => new StatusChangedNotification(buffer),
                _ => new NotSupportedNotification(buffer, SourceBusName)
            },
            0x63 => db0 switch
            {
                0x21 => new VersionNotification(buffer),
                _ => new NotSupportedNotification(buffer, SourceBusName)
            },
            0x64 => db0 switch
            {
                0x14 => new CVOkResponse(buffer),
                _ => new NotSupportedNotification(buffer, SourceBusName)
            },
            0x81 => db0 switch
            {
                0x00 => new EmergencyStopBroadcast(),
                _ => new NotSupportedNotification(buffer, SourceBusName)
            },
            0xEF => new LocoInfoNotification(buffer),
            0xF3 => db0 switch
            {
                0x0A => new FirmwareNotification(buffer),
                _ => new NotSupportedNotification(buffer, SourceBusName)
            },
            _ => new NotSupportedNotification(buffer, SourceBusName),
        };
    }
}
