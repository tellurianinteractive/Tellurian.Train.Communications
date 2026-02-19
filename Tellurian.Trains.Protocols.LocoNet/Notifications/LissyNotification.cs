using System.Globalization;

namespace Tellurian.Trains.Protocols.LocoNet.Notifications;

/// <summary>
/// OPC_LISSY_UPDATE (0xE4) - LISSY/RailCom locomotive identification report.
/// Reports locomotive address, direction, and category from LISSY or RailCom-enabled detectors.
/// </summary>
public sealed class LissyNotification : Notification
{
    public const byte OperationCode = 0xE4;
    private const byte LissySubType = 0x08;

    internal LissyNotification(byte[] data)
    {
        if (data is null || data.Length < 7)
            throw new ArgumentException("LISSY notification must be at least 7 bytes", nameof(data));

        ValidateData(OperationCode, data);

        // data[1] = message length / sub-type identifier
        // data[2] = section address
        SectionAddress = data[2];

        // data[3] = loco address high (7 bits)
        // data[4] = loco address low (7 bits)
        LocoAddress = (ushort)(((data[3] & 0x7F) << 7) | (data[4] & 0x7F));

        // data[5] contains direction and category
        // Bit 5: direction (1 = forward, 0 = reverse)
        IsForward = (data[5] & 0x20) != 0;

        // Bits 4-0: category
        Category = (byte)(data[5] & 0x1F);

        IsValid = LocoAddress > 0;
    }

    /// <summary>
    /// The locomotive address reported by LISSY/RailCom.
    /// </summary>
    public ushort LocoAddress { get; }

    /// <summary>
    /// True if the report contains a valid locomotive address.
    /// </summary>
    public bool IsValid { get; }

    /// <summary>
    /// True if the locomotive is moving forward.
    /// </summary>
    public bool IsForward { get; }

    /// <summary>
    /// LISSY category/classification value.
    /// </summary>
    public byte Category { get; }

    /// <summary>
    /// The section address of the LISSY detector.
    /// </summary>
    public byte SectionAddress { get; }

    /// <summary>
    /// Checks whether the given data represents a LISSY update message.
    /// Discriminates from other messages sharing the 0xE4 opcode.
    /// </summary>
    internal static bool IsLissyMessage(byte[] data) =>
        data.Length >= 2 && data[1] == LissySubType;

    public override string ToString() =>
        string.Format(CultureInfo.InvariantCulture,
            "LISSY Section {0}: Loco {1} {2} Category {3}",
            SectionAddress, LocoAddress, IsForward ? "Fwd" : "Rev", Category);
}
