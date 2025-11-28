namespace Tellurian.Trains.Protocols.LocoNet.Commands;

public sealed class SetLocoSpeedCommand : Command
{
    public const byte OperationCode = 0xA0;
    private readonly byte[] data;

    public static SetLocoSpeedCommand Zero(byte slot) => new(slot, 0);
    public static SetLocoSpeedCommand Stop(byte slot) => new(slot, 1);

    public SetLocoSpeedCommand(byte slot, byte speedSteps)
    {
        data = new byte[3];
        data[0] = OperationCode;
        data[1] = slot;
        data[2] = speedSteps;
    }
    public override byte[] GetBytesWithChecksum() => AppendChecksum(data);


}

public class SetLocoDirectionAndFunctionF0toF4Command : Command
{
    public const byte OperationCode = 0xA1;
    private readonly byte[] Data;

    public SetLocoDirectionAndFunctionF0toF4Command(byte slot, bool forward, bool F0, bool F1, bool F2, bool F3, bool F4)
    {
        Data = new byte[3];
        Data[0] = OperationCode;
        Data[1] = slot;
        Data[2] = (byte)((forward ? 0x20 : 0x00) + (F0 ? 0x10 : 0x00) + (F1 ? 0x08 : 0x00) + (F2 ? 0x04 : 0x00) + (F3 ? 0x02 : 0x00) + (F4 ? 0x01 : 0x00));
    }

    public SetLocoDirectionAndFunctionF0toF4Command(byte[] data)
    {
        if (data != null && data.Length == 4) Data = data[..3]; else throw new ArgumentNullException(nameof(data));
    }

    public override byte[] GetBytesWithChecksum() => AppendChecksum(Data);
}

public class SetLocoFunctionF5toF8Command : Command
{
    public const byte OperationCode = 0xA2;
    private readonly byte[] Data;

    public SetLocoFunctionF5toF8Command(byte slot, bool F5, bool F6, bool F7, bool F8)
    {
        Data = new byte[3];
        Data[0] = OperationCode;
        Data[1] = slot;
        Data[2] = (byte)((F8 ? 0x08 : 0x00) + (F7 ? 0x04 : 0x00) + (F6 ? 0x02 : 0x00) + (F5 ? 0x01 : 0x00));
    }

    public SetLocoFunctionF5toF8Command(byte[] data)
    {
        if (data != null && data.Length == 4) Data = data[..3]; else throw new ArgumentNullException(nameof(data));
    }

    public override byte[] GetBytesWithChecksum() => AppendChecksum(Data);
}

public class SetLocoFunctionF9toF12 : Command
{
    private readonly byte[] Data;
    public const byte OperationCode = 0xA3;

    public SetLocoFunctionF9toF12(byte slot, bool F9, bool F10, bool F11, bool F12)
    {
        Data = new byte[3];
        Data[0] = OperationCode;
        Data[1] = slot;
        Data[2] = (byte)((F12 ? 0x08 : 0x00) + (F11 ? 0x04 : 0x00) + (F10 ? 0x02 : 0x00) + (F9 ? 0x01 : 0x00));
    }
    public override byte[] GetBytesWithChecksum() => AppendChecksum(Data);

}

public class SetLocoFunctionF13toF19 : Command
{
    private readonly byte[] Data;
    public const byte OperationCode = 0xD4;

    public SetLocoFunctionF13toF19(byte slot, bool F13, bool F14, bool F15, bool F16, bool F17, bool F18, bool F19)
    {
        Data = new byte[5];
        Data[0] = OperationCode;
        Data[1] = 0x20;
        Data[2] = slot;
        Data[3] = 0x08;
        Data[4] = (byte)((F19 ? 0x40 : 0x00) + (F18 ? 0x20 : 0x00) + (F17 ? 0x10 : 0x00) + (F16 ? 0x08 : 0x00) + (F15 ? 0x04 : 0x00) + (F14 ? 0x02 : 0x00) + (F13 ? 0x01 : 0x00));
    }
    public override byte[] GetBytesWithChecksum() => AppendChecksum(Data);
}


