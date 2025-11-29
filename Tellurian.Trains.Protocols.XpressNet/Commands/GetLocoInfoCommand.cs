using Tellurian.Trains.Interfaces.Locos;

namespace Tellurian.Trains.Protocols.XpressNet.Commands;

public sealed class GetLocoInfoCommand : Command
{
    public GetLocoInfoCommand(Address address) : base(0xE0, GetData(address)) { }

    private static byte[] GetData(Address address)
    {
        var result = new byte[3];
        result[0] = 0xF0;
        Array.Copy(address.GetBytesAccordingToXpressNet(), 0, result, 1, 2);
        return result;
    }
}
