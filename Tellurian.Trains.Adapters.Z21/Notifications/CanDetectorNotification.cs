namespace Tellurian.Trains.Adapters.Z21;

/// <summary>
/// LAN_CAN_DETECTOR (0xC4) notification from Z21.
/// Reports CAN bus detector state from Z21 10808 and similar modules.
/// </summary>
public sealed class CanDetectorNotification : Notification
{
    internal CanDetectorNotification(Frame frame) : base(frame)
    {
        var data = frame.Data;
        if (data.Length < 10) return;

        NetworkId = BitConverter.ToUInt16(data, 0);
        ModuleAddress = BitConverter.ToUInt16(data, 2);
        Port = data[4];
        DetectorType = data[5];
        Value1 = BitConverter.ToUInt16(data, 6);
        Value2 = BitConverter.ToUInt16(data, 8);

        if (DetectorType == 0x01)
        {
            OccupancyStatus = (CanOccupancyStatus)Value1;
            IsOccupied = (Value1 & 0x1000) != 0;
        }
        else if (DetectorType >= 0x11 && DetectorType <= 0x1F)
        {
            // RailCom address data
            IsRailCom = true;
            LocoAddress1 = (ushort)(Value1 & 0x3FFF);
            Direction1IsForward = (Value1 & 0x8000) != 0;
            Direction1IsValid = (Value1 & 0x4000) != 0;
            LocoAddress2 = (ushort)(Value2 & 0x3FFF);
            Direction2IsForward = (Value2 & 0x8000) != 0;
            Direction2IsValid = (Value2 & 0x4000) != 0;
        }
    }

    /// <summary>
    /// Network ID of the CAN detector module.
    /// </summary>
    public ushort NetworkId { get; }

    /// <summary>
    /// Module address on the CAN bus.
    /// </summary>
    public ushort ModuleAddress { get; }

    /// <summary>
    /// Port number (0-7) on the detector module.
    /// </summary>
    public byte Port { get; }

    /// <summary>
    /// Detector type byte. 0x01 = occupancy, 0x11-0x1F = RailCom addresses.
    /// </summary>
    public byte DetectorType { get; }

    /// <summary>
    /// Raw value 1 from the notification.
    /// </summary>
    public ushort Value1 { get; }

    /// <summary>
    /// Raw value 2 from the notification.
    /// </summary>
    public ushort Value2 { get; }

    /// <summary>
    /// The occupancy status. Valid when <see cref="DetectorType"/> is 0x01.
    /// </summary>
    public CanOccupancyStatus OccupancyStatus { get; }

    /// <summary>
    /// True if the section is occupied. Valid when <see cref="DetectorType"/> is 0x01.
    /// </summary>
    public bool IsOccupied { get; }

    /// <summary>
    /// True if this is a RailCom address report (types 0x11-0x1F).
    /// </summary>
    public bool IsRailCom { get; }

    /// <summary>
    /// First locomotive address from RailCom (14-bit). Valid when <see cref="IsRailCom"/> is true.
    /// </summary>
    public ushort LocoAddress1 { get; }

    /// <summary>
    /// True if direction for first address is forward. Valid when <see cref="Direction1IsValid"/> is true.
    /// </summary>
    public bool Direction1IsForward { get; }

    /// <summary>
    /// True if direction information for first address is available.
    /// </summary>
    public bool Direction1IsValid { get; }

    /// <summary>
    /// Second locomotive address from RailCom (14-bit). Valid when <see cref="IsRailCom"/> is true.
    /// </summary>
    public ushort LocoAddress2 { get; }

    /// <summary>
    /// True if direction for second address is forward. Valid when <see cref="Direction2IsValid"/> is true.
    /// </summary>
    public bool Direction2IsForward { get; }

    /// <summary>
    /// True if direction information for second address is available.
    /// </summary>
    public bool Direction2IsValid { get; }

    public override string ToString() => DetectorType switch
    {
        0x01 => $"CAN {NetworkId}:{ModuleAddress}.{Port}: {OccupancyStatus}",
        >= 0x11 and <= 0x1F => $"CAN {NetworkId}:{ModuleAddress}.{Port}: RailCom Loco1={LocoAddress1} Loco2={LocoAddress2}",
        _ => $"CAN {NetworkId}:{ModuleAddress}.{Port}: Type 0x{DetectorType:X2}"
    };
}
