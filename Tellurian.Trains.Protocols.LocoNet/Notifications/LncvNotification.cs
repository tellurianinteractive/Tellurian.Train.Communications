using Tellurian.Trains.Protocols.LocoNet.Lncv;

namespace Tellurian.Trains.Protocols.LocoNet.Notifications;

/// <summary>
/// Type of LNCV message received.
/// </summary>
public enum LncvMessageType
{
    /// <summary>Reply containing a CV number and its value.</summary>
    ReadReply,
    /// <summary>Acknowledgment that a programming session was opened.</summary>
    SessionAcknowledgment
}

/// <summary>
/// Incoming LNCV reply notification (opcode 0xE5, 15 bytes).
/// </summary>
/// <remarks>
/// Byte layout:
/// [0] 0xE5  [1] 0x0F  [2] SRC  [3] DST_L=0x49  [4] DST_H=0x4B
/// [5] CMD=0x1F  [6] PXCT1  [7] ART_L  [8] ART_H  [9] CVN_L/0  [10] CVN_H/0
/// [11] VAL_L/MOD_L  [12] VAL_H/MOD_H  [13] CMDDATA  [14] checksum
/// </remarks>
public sealed class LncvNotification : Notification
{
    public const byte OperationCode = 0xE5;
    internal const byte ExpectedLength = 0x0F;
    internal const byte DestinationLow = 0x49;
    internal const byte DestinationHigh = 0x4B;
    private const byte CmdReply = 0x1F;
    private const byte CmdDataPron = 0x80;

    internal LncvNotification(byte[] data)
    {
        if (data is null || data.Length != 15)
            throw new ArgumentException("LNCV notification must be exactly 15 bytes", nameof(data));

        ValidateData(OperationCode, data);

        // Extract PXCT1 and data bytes
        var pxct1 = data[6];
        byte[] dataBytes = [data[7], data[8], data[9], data[10], data[11], data[12], data[13]];
        Pxct1Encoding.Decode(pxct1, dataBytes);

        ArticleNumber = (ushort)(dataBytes[0] | (dataBytes[1] << 8));

        if ((dataBytes[6] & CmdDataPron) != 0)
        {
            LncvType = LncvMessageType.SessionAcknowledgment;
            ModuleAddress = (ushort)(dataBytes[4] | (dataBytes[5] << 8));
        }
        else
        {
            LncvType = LncvMessageType.ReadReply;
            CvNumber = (ushort)(dataBytes[2] | (dataBytes[3] << 8));
            CvValue = (ushort)(dataBytes[4] | (dataBytes[5] << 8));
        }
    }

    public LncvMessageType LncvType { get; }
    public ushort ArticleNumber { get; }
    public ushort CvNumber { get; }
    public ushort CvValue { get; }
    public ushort ModuleAddress { get; }

    /// <summary>
    /// Checks whether the given data represents an LNCV peer transfer message.
    /// </summary>
    internal static bool IsLncvMessage(byte[] data) =>
        data.Length >= 5 && data[1] == ExpectedLength && data[3] == DestinationLow && data[4] == DestinationHigh;

    public override string ToString() => LncvType switch
    {
        LncvMessageType.ReadReply => $"LNCV Read Reply: Article {ArticleNumber}, LNCV{CvNumber}={CvValue}",
        LncvMessageType.SessionAcknowledgment => $"LNCV Session Ack: Article {ArticleNumber}, Module {ModuleAddress}",
        _ => $"LNCV Unknown: Article {ArticleNumber}"
    };
}
