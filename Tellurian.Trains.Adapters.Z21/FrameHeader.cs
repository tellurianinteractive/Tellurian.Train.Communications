namespace Tellurian.Trains.Adapters.Z21;

#pragma warning disable CA1028, RCS1234 
public enum FrameHeader : ushort
{
    Undefined = 0,
    SerialNumber = 0x10,
    HardwareInfo = 0x1A,
    Logoff = 0x30,
    Xbus = 0x40,
    SubscribeNotifications = 0x50,
    BroadcastSubjects = 0x51,
    LocoAddressMode = 0x60,
    SetLocomotiveAddressMode = 0x61,
    TurnoutAddressMode = 0x70,
    SetTurnoutAddressMode = 0x71,
    SystemStateChanged = 0x84,
    SystemState = 0x85,
    LocoNetReceive = 0xA0,
    LocoNetTransmit = 0xA1,
    LocoNetCommand = 0xA2,
    LocoNetDispatch = 0xA3,
    Test = 0xFF
}
