namespace Tellurian.Trains.Protocols.XpressNet.Commands;

public abstract class Command : Message {
    protected Command(byte header) : base(header) { }

    protected Command(byte header, byte data) : base(header, data) { }

    protected Command(byte header, byte[] data) : base(header, data) { }

    protected Command(Func<byte[], byte> headerFunc, byte[] data) : base(headerFunc, data) { }

    protected Command(Func<byte[], byte> headerFunc, byte[] data, Func<byte[], bool> lengthFunc): base(headerFunc, data, lengthFunc) { }

    public byte[] GetData()
    {
        var result = new byte[Data.Length + 1];
        result[0] = (byte)((Header & 0xF0) + Length);
        Array.Copy(Data, 0, result, 1, Data.Length);
        return result;
    }
}
