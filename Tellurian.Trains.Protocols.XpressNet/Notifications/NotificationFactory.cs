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
            // Headers 0x41-0x47 are FeedbackBroadcast with N pairs (N=header-0x40)
            // Header 0x42 with only 2 data bytes is AccessoryDecoderInfoNotification (response to request)
            0x41 => new FeedbackBroadcast(buffer),
            0x42 => CreateAccessoryNotification(buffer),
            0x43 => new FeedbackBroadcast(buffer),
            0x44 => new FeedbackBroadcast(buffer),
            0x45 => new FeedbackBroadcast(buffer),
            0x46 => new FeedbackBroadcast(buffer),
            0x47 => new FeedbackBroadcast(buffer),
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
                0x80 => new TransferErrorNotification(),
                0x81 => new CommandStationBusyNotification(),
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
                0x10 => new ServiceModeRegisterPagedNotification(buffer),
                0x14 => new ServiceModeDirectCVNotification(buffer),
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
            0xE1 => IsMUDHError(db0)
                ? new MUDHErrorNotification(buffer)
                : new NotSupportedNotification(buffer, SourceBusName),
            0xE3 => Create0xE3Notification(buffer, db0),
            0xEF => new LocoInfoNotification(buffer),
            0xF3 => db0 switch
            {
                0x0A => new FirmwareNotification(buffer),
                _ => new NotSupportedNotification(buffer, SourceBusName)
            },
            _ => new NotSupportedNotification(buffer, SourceBusName),
        };
    }

    /// <summary>
    /// Disambiguates between AccessoryDecoderInfoNotification and FeedbackBroadcast for header 0x42.
    /// </summary>
    /// <remarks>
    /// Header 0x42 can be either:
    /// - AccessoryDecoderInfoNotification (response to request): 3 bytes total (header + 2 data bytes)
    /// - FeedbackBroadcast with 2 pairs: 5 bytes total (header + 4 data bytes)
    /// </remarks>
    private static Notification CreateAccessoryNotification(byte[] buffer) =>
        buffer.Length == 3
            ? new AccessoryDecoderInfoNotification(buffer)
            : new FeedbackBroadcast(buffer);

    /// <summary>
    /// Checks if the identification byte indicates a MU/DH error (0x81-0x88).
    /// </summary>
    private static bool IsMUDHError(byte identification) =>
        identification >= 0x81 && identification <= 0x88;

    /// <summary>
    /// Creates the appropriate notification for header 0xE3.
    /// </summary>
    private static Notification Create0xE3Notification(byte[] buffer, byte db0) => db0 switch
    {
        // 0x30-0x34: Address retrieval response (K=0-4)
        >= 0x30 and <= 0x34 => new AddressRetrievalNotification(buffer),
        // 0x40: Loco operated by another device
        0x40 => new LocoOperatedByAnotherDeviceNotification(buffer),
        // 0x50: Function status response
        0x50 => new FunctionStatusNotification(buffer),
        _ => new NotSupportedNotification(buffer, SourceBusName)
    };
}
