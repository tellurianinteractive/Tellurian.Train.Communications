using Tellurian.Trains.Interfaces.Extensions;
using Tellurian.Trains.Protocols.XpressNet.Commands;

namespace Tellurian.Trains.Protocols.XpressNet.Decoder;

public sealed class ReadCVCommand : Command
{
    public ReadCVCommand(CvAddress cv) : base(0x23, GetData(cv)) { }
    private static byte[] GetData(CvAddress cv)
    {
        var data = new byte[3];
        data[0] = 0x11;
        data[1] = cv.MSB;
        data[2] = cv.LSB;
        return data;
    }
}
