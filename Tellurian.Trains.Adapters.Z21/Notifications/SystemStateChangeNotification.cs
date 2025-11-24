using Tellurian.Trains.Protocols.XpressNet;

namespace Tellurian.Trains.Adapters.Z21;

/// <summary>
/// Sent from Z21 as a response on <see cref="GetSystemStateCommand"/>.
/// </summary>
/// <remarks>
/// Reference: Z21 LAN Protokoll Spezifikation 2.18
/// </remarks>
public sealed class SystemStateChangeNotification : Notification
{
    internal SystemStateChangeNotification(Frame frame) : base(frame)
    {
        var data = frame.Data;
        MainCurrent = BitConverter.ToInt16(data, 0) / 1000; // Ampere
        ProgramTrackCurrent = BitConverter.ToInt16(data, 2) / 1000; // Ampere
        FilteredMainCurrent = BitConverter.ToInt16(data, 4) / 1000; // Ampere
        InternalTemperature = BitConverter.ToInt16(data, 6); // Celsius
        SupplyVoltage = BitConverter.ToInt16(data, 8) / 1000; // Volt
        TrackVoltage = BitConverter.ToInt16(data, 10) / 1000; // Volt
        Status = (CentralStates)data[12];
        ExtendedStatus = (ExtendedCentralStatuses)data[13];
    }

    public float MainCurrent { get; }
    public float ProgramTrackCurrent { get; }
    public float FilteredMainCurrent { get; }
    public int InternalTemperature { get; }
    public float SupplyVoltage { get; }
    public float TrackVoltage { get; }
    public CentralStates Status { get; }
    public ExtendedCentralStatuses ExtendedStatus { get; }
}
