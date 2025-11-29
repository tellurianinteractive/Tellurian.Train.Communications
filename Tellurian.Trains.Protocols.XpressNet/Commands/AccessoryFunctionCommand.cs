using Tellurian.Trains.Interfaces.Accessories;

namespace Tellurian.Trains.Protocols.XpressNet.Commands;

public sealed class AccessoryFunctionCommand : Command
{
    public AccessoryFunctionCommand(Address address, AccessoryOutput output, AccessoryOutputState state) : base(0x50, GetData(address, output, state)) { }
    public AccessoryFunctionCommand(Address address, AccessoryOutput output, AccessoryOutputState state, AccessoryZ21Mode mode) : base(0x50, GetData(address, output, state, mode)) { }

    private static byte[] GetData(Address address, AccessoryOutput output, AccessoryOutputState state)
    {
        var result = new byte[2];
        result[0] = address.Group;
        result[1] = (byte)(0x80 + ((byte)(state == AccessoryOutputState.Off ? 1 : 0) * 4) + (address.Subaddress * 2) + (byte)output);
        return result;
    }

    private static byte[] GetData(Address address, AccessoryOutput output, AccessoryOutputState state, AccessoryZ21Mode mode)
    {
        var result = new byte[3];
        Array.Copy(address.GetBytes(), 0, result, 0, 2);
        result[2] = (byte)(0x80 + ((byte)(mode) * 32) + ((byte)state * 8) + output);
        return result;
    }
}
