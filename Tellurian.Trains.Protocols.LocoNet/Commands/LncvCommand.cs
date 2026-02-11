using Tellurian.Trains.Protocols.LocoNet.Lncv;

namespace Tellurian.Trains.Protocols.LocoNet.Commands;

/// <summary>
/// LNCV programming command (opcode 0xED, 15 bytes).
/// Used for start session, read, and write operations on LNCV devices.
/// </summary>
/// <remarks>
/// Byte layout:
/// [0] 0xED  [1] 0x0F  [2] SRC=0x01  [3] DST_L=0x49  [4] DST_H=0x4B
/// [5] CMD   [6] PXCT1  [7] ART_L  [8] ART_H  [9] CVN_L  [10] CVN_H
/// [11] MOD_L/VAL_L  [12] MOD_H/VAL_H  [13] CMDDATA  [14] checksum
/// </remarks>
public sealed class LncvCommand : Command
{
    public const byte OperationCode = 0xED;
    private const byte MessageLength = 0x0F;
    private const byte Source = 0x01;
    private const byte DestinationLow = 0x49;
    private const byte DestinationHigh = 0x4B;

    private const byte CmdReadOrSession = 0x21;
    private const byte CmdWrite = 0x20;
    private const byte CmdDataPron = 0x80; // Programming ON flag
    private const byte CmdDataNone = 0x00;

    private readonly byte[] _data;

    private LncvCommand(byte[] data)
    {
        _data = data;
    }

    /// <summary>
    /// Creates a start session command for a specific module or broadcast discovery.
    /// </summary>
    /// <param name="articleNumber">Product code of the target device.</param>
    /// <param name="moduleAddress">Module address, or 0xFFFF for broadcast discovery.</param>
    public static LncvCommand StartSession(ushort articleNumber, ushort moduleAddress) =>
        Create(CmdReadOrSession, articleNumber, cvNumber: 0, moduleOrValue: moduleAddress, cmdData: CmdDataPron);

    /// <summary>
    /// Creates a read LNCV command.
    /// </summary>
    /// <param name="articleNumber">Product code of the target device.</param>
    /// <param name="cvNumber">The LNCV number to read (0-65535).</param>
    /// <param name="moduleAddress">Module address of the target device.</param>
    public static LncvCommand Read(ushort articleNumber, ushort cvNumber, ushort moduleAddress) =>
        Create(CmdReadOrSession, articleNumber, cvNumber, moduleOrValue: moduleAddress, cmdData: CmdDataNone);

    /// <summary>
    /// Creates a write LNCV command.
    /// </summary>
    /// <param name="articleNumber">Product code of the target device.</param>
    /// <param name="cvNumber">The LNCV number to write (0-65535).</param>
    /// <param name="value">The value to write (0-65535).</param>
    public static LncvCommand Write(ushort articleNumber, ushort cvNumber, ushort value) =>
        Create(CmdWrite, articleNumber, cvNumber, moduleOrValue: value, cmdData: CmdDataNone);

    private static LncvCommand Create(byte cmd, ushort articleNumber, ushort cvNumber, ushort moduleOrValue, byte cmdData)
    {
        byte[] dataBytes =
        [
            (byte)(articleNumber & 0xFF),
            (byte)(articleNumber >> 8),
            (byte)(cvNumber & 0xFF),
            (byte)(cvNumber >> 8),
            (byte)(moduleOrValue & 0xFF),
            (byte)(moduleOrValue >> 8),
            cmdData
        ];

        var pxct1 = Pxct1Encoding.Encode(dataBytes);

        byte[] message =
        [
            OperationCode, MessageLength, Source, DestinationLow, DestinationHigh,
            cmd, pxct1,
            dataBytes[0], dataBytes[1], dataBytes[2], dataBytes[3],
            dataBytes[4], dataBytes[5], dataBytes[6]
        ];

        return new LncvCommand(AppendChecksum(message));
    }

    public override byte[] GetBytesWithChecksum() => _data;
}
