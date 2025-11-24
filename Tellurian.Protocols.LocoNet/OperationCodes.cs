namespace Tellurian.Trains.Protocols.LocoNet;
[Obsolete]
internal enum OperationCodes : byte {
    MasterBusyNotification = 0x81,
    GlobalPowerOffCommand = 0x82,
    GlobalPowerOnCommand = 0x83,
    ForceIdleCommand = 0x84,

    SetLocoSpeedCommand = 0xA0,
    SetLocoFunctionCommand = 0xA1,
    SetLocoExtendedFunction = 0xA2,

    TurnoutCommand = 0xB0,
    TurnoutNotification = 0xB1,
    SensorNotification = 0xB2,

    LongAcknowledgeNotification = 0xB4,

    WriteSlotStatCommand = 0xB5,
    SetConsistDirectionCommand = 0xB6,
    UnlinkSlotCommand = 0xB8,
    LinkSlotCommand = 0xB9,
    MoveSlotCommand = 0xBA,
    RequestSlotCommand = 0xBB,

    RequestTurnoutStateCommand = 0xBC,
    SetTurnoutFunctionCommand = 0xBD,
    LocoAddressRequest = 0xBF,

    SlotNotification = 0xE7,
    SlotCommand = 0xEF
}
