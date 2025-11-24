using Tellurian.Trains.Interfaces.Extensions;
using Tellurian.Trains.Protocols.XpressNet.Commands;

namespace Tellurian.Trains.Protocols.XpressNet.Decoder;

public sealed class WriteCVCommand : Command
{
    public WriteCVCommand(CvAddress cv, byte value) : base(0x24, GetData(cv, value)) { }
    private static byte[] GetData(CvAddress cv, byte value)
    {
        var data = new byte[4];
        data[0] = 0x12;
        data[1] = cv.MSB;
        data[2] = cv.LSB;
        data[3] = value;
        return data;
    }
}
