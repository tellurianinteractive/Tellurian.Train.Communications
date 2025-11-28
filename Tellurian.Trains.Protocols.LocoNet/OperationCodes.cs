namespace Tellurian.Trains.Protocols.LocoNet;

/// <summary>
/// LocoNet operation codes (opcodes) as defined in the LocoNet specification.
/// Note: This enum is obsolete. Use the const byte OperationCode in each command/notification class instead.
/// </summary>
[Obsolete("Use const byte OperationCode in each command/notification class instead")]
internal enum OperationCodes : byte
{
    MasterBusyNotification = 0x81,
    GlobalPowerOffCommand = 0x82,
    GlobalPowerOnCommand = 0x83,
    ForceIdleCommand = 0x85,
    SetLocoSpeedCommand = 0xA0,
    SetLocoDirectionCommand = 0xA1,
    SetLocoSoundCommand = 0xA2,
    SwitchRequestCommand = 0xB0,
    SwitchReportNotification = 0xB1,
    SensorInputNotification = 0xB2,
    LongAcknowledgeNotification = 0xB4,
    WriteSlotStatCommand = 0xB5,
    ConsistFunctionCommand = 0xB6,
    UnlinkSlotCommand = 0xB8,
    LinkSlotCommand = 0xB9,
    MoveSlotCommand = 0xBA,
    RequestSlotDataCommand = 0xBB,
    RequestSwitchStateCommand = 0xBC,
    SwitchAcknowledgeCommand = 0xBD,
    LocoAddressRequest = 0xBF,
    SlotReadNotification = 0xE7,
    ImmediatePacketCommand = 0xED,
    WriteSlotDataCommand = 0xEF
}
