using Tellurian.Trains.Protocols.LocoNet.Lncv;

namespace Tellurian.Trains.Protocols.LocoNet.Commands;

/// <summary>
/// LNCV end session command (opcode 0xE5, 15 bytes).
/// Ends an LNCV programming session.
/// </summary>
/// <remarks>
/// Uses the reply opcode 0xE5 per Uhlenbrock specification.
/// </remarks>
public sealed class LncvEndSessionCommand : Command
{
    public const byte OperationCode = 0xE5;
    private const byte MessageLength = 0x0F;
    private const byte Source = 0x01;
    private const byte DestinationLow = 0x49;
    private const byte DestinationHigh = 0x4B;
    private const byte Cmd = 0x21;
    private const byte CmdDataProff = 0x40; // Programming OFF flag

    private readonly byte[] _data;

    public LncvEndSessionCommand(ushort articleNumber, ushort moduleAddress)
    {
        byte[] dataBytes =
        [
            (byte)(articleNumber & 0xFF),
            (byte)(articleNumber >> 8),
            0x00, // CV number low (not used)
            0x00, // CV number high (not used)
            (byte)(moduleAddress & 0xFF),
            (byte)(moduleAddress >> 8),
            CmdDataProff
        ];

        var pxct1 = Pxct1Encoding.Encode(dataBytes);

        byte[] message =
        [
            OperationCode, MessageLength, Source, DestinationLow, DestinationHigh,
            Cmd, pxct1,
            dataBytes[0], dataBytes[1], dataBytes[2], dataBytes[3],
            dataBytes[4], dataBytes[5], dataBytes[6]
        ];

        _data = AppendChecksum(message);
    }

    public override byte[] GetBytesWithChecksum() => _data;
}
