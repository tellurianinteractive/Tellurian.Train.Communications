namespace Tellurian.Trains.Adapters.Z21;

public abstract class SetAddressModeCommand : Command
{
    private readonly short Address;
    private readonly AddressMode Mode;
    protected SetAddressModeCommand(short address, AddressMode mode)
    {
        Address = address;
        Mode = mode;
    }

    protected byte[] GetData()
    {
        var data = new byte[3];
        var address = BitConverterExtensions.GetBigEndianBytes(Address);
        Buffer.BlockCopy(address, 0, data, 0, address.Length);
        data[2] = (byte)Mode;
        return data;
    }
}
