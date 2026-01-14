using Tellurian.Trains.Communications.Interfaces.Decoder;
using Tellurian.Trains.Protocols.XpressNet.Commands;

namespace Tellurian.Trains.Protocols.XpressNet.Decoder;

/// <summary>
/// Command to write a CV value in service mode (programming track).
/// </summary>
public sealed class WriteCVCommand : Command
{
    /// <summary>
    /// Creates a write CV command.
    /// </summary>
    /// <param name="cv">CV with number (1-1024) and value (0-255)</param>
    public WriteCVCommand(CV cv) : base(0x24, GetData(cv)) { }

    private static byte[] GetData(CV cv)
    {
        var (msb, lsb) = EncodeCvNumber(cv.Number);
        return [0x12, msb, lsb, cv.Value];
    }

    /// <summary>
    /// Encodes CV number (1-1024) to MSB/LSB wire format (0-1023).
    /// </summary>
    private static (byte msb, byte lsb) EncodeCvNumber(int cvNumber)
    {
        var wireValue = cvNumber - 1;
        return ((byte)(wireValue >> 8), (byte)(wireValue & 0xFF));
    }
}
