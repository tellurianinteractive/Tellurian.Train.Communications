namespace Tellurian.Trains.Adapters.Z21;

/// <summary>
/// LAN_LOCONET_DETECTOR (0xA4) notification from Z21.
/// Reports detector state via the Z21 LocoNet detector API.
/// </summary>
public sealed class LocoNetDetectorNotification : Notification
{
    internal LocoNetDetectorNotification(Frame frame) : base(frame)
    {
        var data = frame.Data;
        if (data.Length < 3) return;

        DetectorType = data[0];
        FeedbackAddress = BitConverter.ToUInt16(data, 1);

        switch (DetectorType)
        {
            case 0x01: // Occupancy status
                if (data.Length >= 4) IsOccupied = data[3] != 0;
                break;
            case 0x02: // Transponder entering
                IsTransponder = true;
                IsEntering = true;
                if (data.Length >= 5) TransponderAddress = (ushort)(BitConverter.ToUInt16(data, 3) & 0x3FFF);
                break;
            case 0x03: // Transponder leaving
                IsTransponder = true;
                IsEntering = false;
                if (data.Length >= 5) TransponderAddress = (ushort)(BitConverter.ToUInt16(data, 3) & 0x3FFF);
                break;
            case 0x10: // LISSY loco address with direction
                IsLissy = true;
                if (data.Length >= 5)
                {
                    LocoAddress = (ushort)(BitConverter.ToUInt16(data, 3) & 0x3FFF);
                    HasDirection = (data[4] & 0x80) != 0;
                    IsForward = (data[4] & 0x40) != 0;
                    ClassInfo = (byte)(data[4] & 0x3F);
                }
                break;
            case 0x11: // LISSY block status
                if (data.Length >= 4) IsOccupied = data[3] != 0;
                break;
            case 0x12: // LISSY speed
                if (data.Length >= 5) Speed = BitConverter.ToUInt16(data, 3);
                break;
        }
    }

    /// <summary>
    /// The detector type byte.
    /// </summary>
    public byte DetectorType { get; }

    /// <summary>
    /// The feedback address reported by the detector.
    /// </summary>
    public ushort FeedbackAddress { get; }

    /// <summary>
    /// True if the section is occupied. Valid for types 0x01 and 0x11.
    /// </summary>
    public bool IsOccupied { get; }

    /// <summary>
    /// True if this is a transponder event (types 0x02/0x03).
    /// </summary>
    public bool IsTransponder { get; }

    /// <summary>
    /// True if a transponder is entering. Valid when <see cref="IsTransponder"/> is true.
    /// </summary>
    public bool IsEntering { get; }

    /// <summary>
    /// The transponder address. Valid when <see cref="IsTransponder"/> is true.
    /// </summary>
    public ushort TransponderAddress { get; }

    /// <summary>
    /// True if this is a LISSY/RailCom identification (type 0x10).
    /// </summary>
    public bool IsLissy { get; }

    /// <summary>
    /// The locomotive address from LISSY/RailCom. Valid when <see cref="IsLissy"/> is true.
    /// </summary>
    public ushort LocoAddress { get; }

    /// <summary>
    /// True if direction information is available. Valid when <see cref="IsLissy"/> is true.
    /// </summary>
    public bool HasDirection { get; }

    /// <summary>
    /// True if forward direction. Valid when <see cref="HasDirection"/> is true.
    /// </summary>
    public bool IsForward { get; }

    /// <summary>
    /// Classification info byte from LISSY. Valid when <see cref="IsLissy"/> is true.
    /// </summary>
    public byte ClassInfo { get; }

    /// <summary>
    /// Speed value. Valid for type 0x12.
    /// </summary>
    public ushort Speed { get; }

    public override string ToString() => DetectorType switch
    {
        0x01 or 0x11 => $"LocoNetDetector {FeedbackAddress}: {(IsOccupied ? "Occupied" : "Free")}",
        0x02 or 0x03 => $"LocoNetDetector {FeedbackAddress}: Transponder {TransponderAddress} {(IsEntering ? "Entering" : "Leaving")}",
        0x10 => $"LocoNetDetector {FeedbackAddress}: LISSY Loco {LocoAddress}",
        0x12 => $"LocoNetDetector {FeedbackAddress}: Speed {Speed}",
        _ => $"LocoNetDetector {FeedbackAddress}: Type 0x{DetectorType:X2}"
    };
}
