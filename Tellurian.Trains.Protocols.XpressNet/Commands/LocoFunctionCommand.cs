using Tellurian.Trains.Communications.Interfaces.Locos;

namespace Tellurian.Trains.Protocols.XpressNet.Commands;

public sealed class LocoFunctionCommand : Command
{
    public LocoFunctionCommand(Address address, byte function, LocoFunctionStates state) : base(0xE0, GetData(address, function, state)) { }

    private static byte[] GetData(Address address, byte function, LocoFunctionStates state)
    {
        var result = new byte[4];
        result[0] = 0xF8;
        Array.Copy(address.GetBytesAccordingToXpressNet(), 0, result, 1, 2);
        result[3] = (byte)(function | (byte)state);
        return result;
    }
}
