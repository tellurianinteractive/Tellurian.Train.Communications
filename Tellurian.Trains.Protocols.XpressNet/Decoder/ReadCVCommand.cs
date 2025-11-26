using Tellurian.Trains.Protocols.XpressNet.Commands;

namespace Tellurian.Trains.Protocols.XpressNet.Decoder;

/// <summary>
/// Command to read a CV value in service mode (programming track).
/// </summary>
/// <remarks>
/// Creates a read CV command.
/// </remarks>
/// <param name="cvNumber">CV number (1-1024)</param>
public sealed class ReadCVCommand(int cvNumber) : Command(0x23, GetData(cvNumber))
{
    private static byte[] GetData(int cvNumber)
    {
        ValidateCvNumber(cvNumber);
        var (msb, lsb) = EncodeCvNumber(cvNumber);
        return [0x11, msb, lsb];
    }

    private static void ValidateCvNumber(int cvNumber)
    {
        if (cvNumber < 1 || cvNumber > 1024)
            throw new ArgumentOutOfRangeException(nameof(cvNumber), "CV number must be between 1 and 1024");
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
