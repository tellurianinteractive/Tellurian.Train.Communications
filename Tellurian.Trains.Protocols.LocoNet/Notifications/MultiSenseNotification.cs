using System.Globalization;

namespace Tellurian.Trains.Protocols.LocoNet.Notifications;

/// <summary>
/// OPC_MULTI_SENSE (0xD0) - Multi-sensor report for transponding and power management.
/// Reports transponder present/absent events and BDL16x power management messages.
/// </summary>
public sealed class MultiSenseNotification : Notification
{
    public const byte OperationCode = 0xD0;

    private const byte TranspondingPresentMask = 0x20;
    private const byte TranspondingAbsentMask = 0x00;
    private const byte TypeMask = 0x60;
    private const byte PowerManagementMask = 0x60;

    internal MultiSenseNotification(byte[] data)
    {
        if (data is null || data.Length != 6)
            throw new ArgumentException("MultiSense message must be exactly 6 bytes", nameof(data));

        ValidateData(OperationCode, data);

        byte type = (byte)(data[1] & TypeMask);
        byte zoneBits = (byte)(data[2] & 0x0F);

        // Section address from bits of data[1] and data[2]
        int highAddress = (data[1] & 0x1F) << 7;
        int lowAddress = data[2] & 0x7F;
        Section = (ushort)(highAddress | lowAddress);

        // Zone is encoded in lower nibble of data[2]: 0-7 maps to A-H
        Zone = (char)('A' + (zoneBits & 0x07));

        if (type == TranspondingPresentMask)
        {
            IsTransponding = true;
            IsPresent = true;
            LocoAddress = DecodeLocoAddress(data[3], data[4]);
        }
        else if (type == TranspondingAbsentMask)
        {
            IsTransponding = true;
            IsPresent = false;
            LocoAddress = DecodeLocoAddress(data[3], data[4]);
        }
        else if (type == PowerManagementMask)
        {
            IsTransponding = false;
            IsPowerMessage = true;
        }
        else
        {
            IsTransponding = false;
        }
    }

    /// <summary>
    /// True if this is a transponding event (present or absent).
    /// </summary>
    public bool IsTransponding { get; }

    /// <summary>
    /// True if the transponder is present (entering), false if absent (leaving).
    /// Only valid when <see cref="IsTransponding"/> is true.
    /// </summary>
    public bool IsPresent { get; }

    /// <summary>
    /// True if this is a power management message from BDL16x.
    /// </summary>
    public bool IsPowerMessage { get; }

    /// <summary>
    /// Section address derived from the message bytes.
    /// </summary>
    public ushort Section { get; }

    /// <summary>
    /// Detection zone within the section (A-H).
    /// </summary>
    public char Zone { get; }

    /// <summary>
    /// The locomotive address detected. Only valid when <see cref="IsTransponding"/> is true.
    /// </summary>
    public ushort LocoAddress { get; }

    private static ushort DecodeLocoAddress(byte d3, byte d4)
    {
        // d3 bit 6-0 = high address bits, d4 bit 6-0 = low address bits
        // Short address: d3 == 0x7D, address = d4
        // Long address: address = (d3 << 7) | d4
        if (d3 == 0x7D)
            return (ushort)(d4 & 0x7F);
        return (ushort)(((d3 & 0x7F) << 7) | (d4 & 0x7F));
    }

    public override string ToString()
    {
        if (IsTransponding)
            return string.Format(CultureInfo.InvariantCulture,
                "MultiSense Section {0} Zone {1}: Loco {2} {3}",
                Section, Zone, LocoAddress, IsPresent ? "Present" : "Absent");
        if (IsPowerMessage)
            return string.Format(CultureInfo.InvariantCulture,
                "MultiSense Section {0}: Power Management", Section);
        return string.Format(CultureInfo.InvariantCulture,
            "MultiSense Section {0}: Unknown type", Section);
    }
}
