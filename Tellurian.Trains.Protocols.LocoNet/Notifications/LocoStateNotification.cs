namespace Tellurian.Trains.Protocols.LocoNet.Notifications;

/// <summary>
/// OPC_LOCO_SPD (0xA0) - Locomotive speed change observed on the bus.
/// Format: [0xA0, SLOT, SPD, CHK]
/// </summary>
public sealed class LocoSpeedNotification : Notification
{
    public const byte OperationCode = 0xA0;

    internal LocoSpeedNotification(byte[] data)
    {
        ValidateData(OperationCode, data);
        Slot = data[1];
        Speed = data[2];
    }

    public byte Slot { get; }
    public byte Speed { get; }
}

/// <summary>
/// OPC_LOCO_DIRF (0xA1) - Locomotive direction and function F0-F4 change observed on the bus.
/// Format: [0xA1, SLOT, DIRF, CHK]
/// DIRF byte: bit5=Direction, bit4=F0, bit3=F4, bit2=F3, bit1=F2, bit0=F1
/// </summary>
public sealed class LocoDirfNotification : Notification
{
    public const byte OperationCode = 0xA1;

    internal LocoDirfNotification(byte[] data)
    {
        ValidateData(OperationCode, data);
        Slot = data[1];
        var dirf = data[2];
        Direction = (dirf & 0x20) != 0;
        F0 = (dirf & 0x10) != 0;
        F4 = (dirf & 0x08) != 0;
        F3 = (dirf & 0x04) != 0;
        F2 = (dirf & 0x02) != 0;
        F1 = (dirf & 0x01) != 0;
    }

    public byte Slot { get; }
    public bool Direction { get; }
    public bool F0 { get; }
    public bool F1 { get; }
    public bool F2 { get; }
    public bool F3 { get; }
    public bool F4 { get; }
}

/// <summary>
/// OPC_LOCO_SND (0xA2) - Locomotive sound function F5-F8 change observed on the bus.
/// Format: [0xA2, SLOT, SND, CHK]
/// SND byte: bit3=F8, bit2=F7, bit1=F6, bit0=F5
/// </summary>
public sealed class LocoSndNotification : Notification
{
    public const byte OperationCode = 0xA2;

    internal LocoSndNotification(byte[] data)
    {
        ValidateData(OperationCode, data);
        Slot = data[1];
        var snd = data[2];
        F8 = (snd & 0x08) != 0;
        F7 = (snd & 0x04) != 0;
        F6 = (snd & 0x02) != 0;
        F5 = (snd & 0x01) != 0;
    }

    public byte Slot { get; }
    public bool F5 { get; }
    public bool F6 { get; }
    public bool F7 { get; }
    public bool F8 { get; }
}
