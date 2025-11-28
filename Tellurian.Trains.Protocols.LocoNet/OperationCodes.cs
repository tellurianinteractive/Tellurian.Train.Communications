namespace Tellurian.Trains.Protocols.LocoNet;

/// <summary>
/// LocoNet operation codes (opcodes) as defined in the LocoNet specification.
/// Note: This enum is obsolete. Use the const byte OperationCode in each command/notification class instead.
/// </summary>
[Obsolete("Use const byte OperationCode in each command/notification class instead")]
internal enum OperationCodes : byte
{
    // 2-byte messages - Power & System Control
    MasterBusyNotification = 0x81,       // OPC_BUSY
    GlobalPowerOffCommand = 0x82,        // OPC_GPOFF
    GlobalPowerOnCommand = 0x83,         // OPC_GPON
    ForceIdleCommand = 0x85,             // OPC_IDLE (emergency stop)

    // 4-byte messages - Locomotive Control
    SetLocoSpeedCommand = 0xA0,          // OPC_LOCO_SPD
    SetLocoDirectionCommand = 0xA1,      // OPC_LOCO_DIRF
    SetLocoSoundCommand = 0xA2,          // OPC_LOCO_SND

    // 4-byte messages - Switch/Turnout Control
    SwitchRequestCommand = 0xB0,         // OPC_SW_REQ
    SwitchReportNotification = 0xB1,     // OPC_SW_REP
    SensorInputNotification = 0xB2,      // OPC_INPUT_REP

    LongAcknowledgeNotification = 0xB4,  // OPC_LONG_ACK

    WriteSlotStatCommand = 0xB5,         // OPC_SLOT_STAT1
    ConsistFunctionCommand = 0xB6,       // OPC_CONSIST_FUNC
    UnlinkSlotCommand = 0xB8,            // OPC_UNLINK_SLOTS
    LinkSlotCommand = 0xB9,              // OPC_LINK_SLOTS
    MoveSlotCommand = 0xBA,              // OPC_MOVE_SLOTS
    RequestSlotDataCommand = 0xBB,       // OPC_RQ_SL_DATA
    RequestSwitchStateCommand = 0xBC,    // OPC_SW_STATE
    SwitchAcknowledgeCommand = 0xBD,     // OPC_SW_ACK
    LocoAddressRequest = 0xBF,           // OPC_LOCO_ADR

    // Variable-length messages
    SlotReadNotification = 0xE7,         // OPC_SL_RD_DATA (14 bytes)
    ImmediatePacketCommand = 0xED,       // OPC_IMM_PACKET (11 bytes)
    WriteSlotDataCommand = 0xEF          // OPC_WR_SL_DATA (14 bytes)
}
